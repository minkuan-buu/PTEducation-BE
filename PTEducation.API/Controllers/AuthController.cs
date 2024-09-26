using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Business.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTEducation.Business.Services.AuthServices;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/authentication")]
    public class AuthController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly IAuthServices _authServices;
        public AuthController(IUserServices userServices, IAuthServices authServices)
        {
            _authServices = authServices;
            _userServices = userServices;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginReqModel User)
        {
            try
            {
                var Result = await _userServices.Login(User.Username, User.Password);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

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

        [HttpPost("change-password")]
        [Authorize]
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
        [Authorize]
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
