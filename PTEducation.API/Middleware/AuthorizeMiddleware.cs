using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PTEducation.Business.ApplicationMiddleware;
using PTEducation.Data.Enums;
using PTEducation.Data.Repositories.UserRepositories;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;

namespace PTEducation.API.Middleware
{
    public class AuthorizeMiddleware : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserRepositories _userRepositories;
        private const string AccessTokenCookieName = "at";
        private const int RefreshWindowDays = 7;

        public AuthorizeMiddleware(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IUserRepositories userRepositories)
            : base(options, logger, encoder, clock)
        {
            _userRepositories = userRepositories;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var requestPath = Context.Request.Path;
            var pathValue = requestPath.Value ?? string.Empty;

            // Allow login, register, and logout endpoints to bypass authentication
            if (pathValue.Contains("/authentication/login", StringComparison.OrdinalIgnoreCase) ||
                pathValue.Contains("/authentication/register", StringComparison.OrdinalIgnoreCase) ||
                pathValue.Contains("/authentication/logout", StringComparison.OrdinalIgnoreCase) ||
                pathValue.Contains("/logout", StringComparison.OrdinalIgnoreCase))
            {
                return AuthenticateResult.NoResult();
            }

            if (!TryGetAccessToken(out var token))
            {
                return AuthenticateResult.Fail("Access token is missing or invalid.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("TestingIssuerSigningKeyPTEducationMS@123");
            ClaimsPrincipal? claimsPrincipal = null;
            SecurityToken? validatedToken = null;
            var shouldIssueNewToken = false;

            try
            {
                claimsPrincipal = tokenHandler.ValidateToken(
                    token,
                    BuildTokenValidationParameters(key, validateLifetime: true),
                    out validatedToken);
            }
            catch (SecurityTokenExpiredException ex)
            {
                if (pathValue.Contains("/authentication/reset-password", StringComparison.OrdinalIgnoreCase))
                {
                    return AuthenticateResult.Fail($"Token has expired: {ex.Message}");
                }

                if (!IsRefreshTokenCandidate(token))
                {
                    return AuthenticateResult.Fail($"Token has expired: {ex.Message}");
                }

                try
                {
                    claimsPrincipal = tokenHandler.ValidateToken(
                        token,
                        BuildTokenValidationParameters(key, validateLifetime: false),
                        out validatedToken);
                }
                catch (SecurityTokenException innerEx)
                {
                    return AuthenticateResult.Fail($"Token validation failed: {innerEx.Message}");
                }

                if (validatedToken is JwtSecurityToken jwtToken &&
                    jwtToken.ValidTo.AddDays(RefreshWindowDays) < DateTime.UtcNow)
                {
                    return AuthenticateResult.Fail("Token refresh window has expired.");
                }

                shouldIssueNewToken = true;
            }
            catch (SecurityTokenException ex)
            {
                return AuthenticateResult.Fail($"Token validation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail($"An error occurred: {ex.Message}");
            }

            if (claimsPrincipal == null)
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            var identity = claimsPrincipal.Identity as ClaimsIdentity;
            if (identity == null || !identity.IsAuthenticated)
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            if (pathValue.Contains("/authentication/reset-password", StringComparison.OrdinalIgnoreCase))
            {
                var typeClaim = identity.FindFirst("type")?.Value;
                if (typeClaim != "reset")
                {
                    return AuthenticateResult.Fail("Invalid token for reset-password.");
                }
            }

            PTEducation.Data.Entities.User? user = null;
            var userIdClaim = identity.FindFirst("userid")?.Value;
            if (userIdClaim != null)
            {
                user = await _userRepositories.GetSingle(u => u.Id == userIdClaim);
                if (user == null || user.Status.Equals(AccountStatusEnums.Inactive.ToString()))
                {
                    return AuthenticateResult.Fail("User is inactive or not found.");
                }
            }

            // Kiểm tra vai trò yêu cầu cho endpoint (nếu có)
            var endpointRoles = GetEndpointRoles();
            var userRoles = identity.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            if (endpointRoles.Any() && !userRoles.Any(ur => endpointRoles.Contains(ur)))
            {
                return AuthenticateResult.Fail("User does not have the required role.");
            }

            if (shouldIssueNewToken && user != null)
            {
                var newToken = Authentication.GenerateJWT(user);
                var encryptedToken = SelfCrypto.Encrypt(newToken);
                var isLocalhost = Context.Request.Host.Host == "localhost";
                Context.Response.Cookies.Append(AccessTokenCookieName, encryptedToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = isLocalhost ? SameSiteMode.None : SameSiteMode.Lax,
                    Domain = isLocalhost ? null : ".pteducation.edu.vn",
                    Expires = DateTimeOffset.UtcNow.AddDays(RefreshWindowDays)
                });
                Context.Response.Headers["X-Access-Token"] = newToken;
            }

            var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

        private bool TryGetAccessToken(out string token)
        {
            if (Request.Cookies.TryGetValue(AccessTokenCookieName, out var encryptedToken))
            {
                var cookieToken = SelfCrypto.Decrypt(encryptedToken);
                if (!string.IsNullOrWhiteSpace(cookieToken))
                {
                    token = cookieToken;
                    return true;
                }
            }

            return TryGetBearerToken(out token);
        }

        private bool TryGetBearerToken(out string token)
        {
            string authorizationHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader) ||
                !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = string.Empty;
                return false;
            }

            token = authorizationHeader.Substring("Bearer ".Length).Trim();
            return !string.IsNullOrWhiteSpace(token);
        }

        private bool IsRefreshTokenCandidate(string expiredToken)
        {
            if (!Request.Cookies.TryGetValue(AccessTokenCookieName, out var encryptedToken))
            {
                return false;
            }

            var cookieToken = SelfCrypto.Decrypt(encryptedToken);
            if (string.IsNullOrWhiteSpace(cookieToken))
            {
                return false;
            }

            return string.Equals(cookieToken, expiredToken, StringComparison.Ordinal);
        }

        private static TokenValidationParameters BuildTokenValidationParameters(byte[] key, bool validateLifetime)
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = "IssuerFromServerhttp://api.pteducation.edu.vn",
                ValidAudience = "AudienceForhttp://tradiem.pteducation.edu.vn",
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        }

        private List<string> GetEndpointRoles()
        {
            var endpoint = Context.GetEndpoint();
            if (endpoint == null)
            {
                return new List<string>();
            }

            var authorizeAttributes = endpoint.Metadata.GetOrderedMetadata<AuthorizeAttribute>();
            var roles = new List<string>();

            foreach (var attribute in authorizeAttributes)
            {
                if (!string.IsNullOrEmpty(attribute.Roles))
                {
                    roles.AddRange(attribute.Roles.Split(',').Select(r => r.Trim()));
                }
            }

            return roles;
        }
    }
}
