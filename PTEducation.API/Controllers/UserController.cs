using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Business.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTEducation.Business.Services.AuthServices;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userServices;
        public UserController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpPost("upload-avatar")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication")]
        public async Task<IActionResult> UploadAvatar([FromForm] AttachmentReqModel reqModel)
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new { message = "User is not authenticated." });
                }
                var Result = await _userServices.UploadAvatar(userId, reqModel);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new { message = "User is not authenticated." });
                }
                var Result = await _userServices.GetMyProfile(userId);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("convert-all")]
        public async Task<IActionResult> ConvertUsersName()
        {
            var Result = await _userServices.ConvertNameFromUnicodeEscapeToUnicode();
            return Ok(Result);
        }
    }
}
