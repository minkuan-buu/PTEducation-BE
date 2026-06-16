using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Business.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTEducation.Business.Services.AuthServices;
using Asp.Versioning;
using AutoMapper;
using PTEducation.Data.DTO.ResponseModel;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/authentication")]
    public class AuthController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserServices _userServices;
        private readonly IAuthServices _authServices;
        public AuthController(IUserServices userServices, IMapper mapper, IAuthServices authServices)
        {
            _authServices = authServices;
            _userServices = userServices;
            _mapper = mapper;
        }


        [MapToApiVersion("2.0")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginReqModel User)
        {
            try
            {
                var RawResult = await _userServices.Login(User.Username, User.Password);
                var isLocalhost = Request.Host.Host == "localhost";
                Response.Cookies.Append("at", RawResult.Data!.EncryptedToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = isLocalhost ? SameSiteMode.None : SameSiteMode.Lax,
                    Domain = isLocalhost ? null : ".pteducation.edu.vn",
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
                var Result = _mapper.Map<DataResultModel<UserLoginResModel>>(RawResult);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [MapToApiVersion("2.0")]
        [HttpPost("logout")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var isLocalhost = Request.Host.Host == "localhost";
                Response.Cookies.Delete("at", new CookieOptions
                {
                    Domain = isLocalhost ? null : ".pteducation.edu.vn",
                    Secure = true,
                    SameSite = isLocalhost ? SameSiteMode.None : SameSiteMode.Lax
                });
                return Ok();
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [MapToApiVersion("1.0")]
        [HttpPost("register")]
        //[Authorize("Admin")]
        public async Task<IActionResult> Register([FromBody] UserRegisterReqModel User)
        {
            try
            {
                var Result = await _userServices.Register(User);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [MapToApiVersion("2.0")]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterForStudentWithGuadianInfo([FromBody] UserRegisterWithGuardianInfo User)
        {
            try
            {
                var Result = await _userServices.Register(User);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [MapToApiVersion("1.0")]
        [HttpPost("change-password")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication")]
        public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordReqModel User)
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var Result = await _userServices.ChangePassword(User, token);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("check-server")]
        [MapToApiVersion("1.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication")]
        public IActionResult CheckServer()
        {
            try
            {
                var Result = _authServices.CheckServer();
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("reset-password")]
        [MapToApiVersion("1.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordReqModel User)
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var Result = await _userServices.ResetPassword(User, token);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //[HttpPost("test")]
        //public async Task<IActionResult> TestEmail()
        //{
        //    try
        //    {
        //        var Result = await _userServices.SendMail();
        //        return Ok(Result);
        //    }
        //    catch (CustomException ex)
        //    {
        //        return BadRequest(new { message = ex.Message });
        //    }
        //}
    }
}
