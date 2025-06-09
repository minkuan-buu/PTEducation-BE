using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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

            // Allow login and register endpoints to bypass authentication
            if (requestPath.StartsWithSegments("/api/authentication/login") || requestPath.StartsWithSegments("/api/authentication/register"))
            {
                return AuthenticateResult.NoResult();
            }

            // Get the Authorization header
            string authorizationHeader = Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                return AuthenticateResult.Fail("Authorization header is missing or invalid.");
            }

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("TestingIssuerSigningKeyPTEducationMS@123");

            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = "IssuerFromServerhttp://api.pteducation.edu.vn",
                    ValidAudience = "AudienceForhttp://tradiem.pteducation.edu.vn",
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken validatedToken);

                if (validatedToken.ValidTo < DateTime.UtcNow)
                {
                    return AuthenticateResult.Fail("Token has expired.");
                }

                var identity = claimsPrincipal.Identity as ClaimsIdentity;
                if (identity == null || !identity.IsAuthenticated)
                {
                    return AuthenticateResult.Fail("Unauthorized");
                }

                if (requestPath.StartsWithSegments("/api/authentication/reset-password"))
                {
                    var typeClaim = identity.FindFirst("type")?.Value;
                    if (typeClaim != "reset")
                    {
                        return AuthenticateResult.Fail("Invalid token for reset-password.");
                    }
                }

                var userIdClaim = identity.FindFirst("userid")?.Value;
                if (userIdClaim != null)
                {
                    var user = await _userRepositories.GetSingle(u => u.Id == userIdClaim);
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

                var ticket = new AuthenticationTicket(claimsPrincipal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            catch (SecurityTokenExpiredException ex)
            {
                return AuthenticateResult.Fail($"Token has expired: {ex.Message}");
            }
            catch (SecurityTokenException ex)
            {
                return AuthenticateResult.Fail($"Token validation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail($"An error occurred: {ex.Message}");
            }
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
