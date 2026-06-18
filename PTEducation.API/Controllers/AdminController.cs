using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Business.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTEducation.Business.Services.AuthServices;
using Org.BouncyCastle.Ocsp;
using PTEducation.Data.DTO.ResponseModel;
using Asp.Versioning;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/admin")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class AdminController : ControllerBase
    {
        private readonly IUserServices _userServices;
        public AdminController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpGet("managers")]
        [MapToApiVersion("1.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetManagers(int? pageIndex, [FromQuery] UserFilter searchModel)
        {
            var userId = User.FindFirst("userid")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }
            var Result = await _userServices.GetManagers(pageIndex, searchModel, userId);
            return Ok(Result);
        }

        [HttpPost("managers")]
        [MapToApiVersion("1.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> RegisterManager([FromBody] List<ManagerRegisterReqModel> ReqModel)
        {
            var Result = await _userServices.Register(ReqModel);
            return Ok(Result);
        }

        [HttpPost("manager/deactivate/{id}")]
        [MapToApiVersion("1.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> DeactivateManager(string id)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var Result = await _userServices.Deactivate(id);
            return Ok(Result);
        }

        [HttpPost("manager/reactivate/{id}")]
        [MapToApiVersion("1.0")]

        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> ReactivateManager(string id)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var Result = await _userServices.ReActivate(id);
            return Ok(Result);
        }

        [HttpPut("student/{studentClassId}")]
        [MapToApiVersion("1.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateStudent(Guid studentClassId, [FromBody] StudentUpdateReqModel reqModel)
        {
            var Result = await _userServices.UpdateStudentInfo(reqModel, studentClassId);
            return Ok(Result);
        }

        [HttpDelete("student/{studentClassId}")]
        [MapToApiVersion("1.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteStudent(Guid studentClassId)
        {
            var Result = await _userServices.DeleteStudent(studentClassId);
            return Ok(Result);
        }

        [HttpGet("students")]
        [MapToApiVersion("2.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllStudents(int? pageIndex, [FromQuery] UserFilter searchModel)
        {
            var Result = await _userServices.GetAllStudents(pageIndex, searchModel);
            return Ok(Result);
        }

        [HttpGet("users/{userId}")]
        [MapToApiVersion("2.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetUserDetail(string userId)
        {
            var Result = await _userServices.GetUserDetail(userId);
            return Ok(Result);
        }

        [HttpPatch("students/{userId}")]
        [MapToApiVersion("2.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateStudent(string userId, [FromBody] AccessReqModel reqModel)
        {
            var Result = await _userServices.UpdateStudentAccess(userId, reqModel);
            return Ok(Result);
        }

        [HttpDelete("students/{userId}")]
        [MapToApiVersion("2.0")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteStudent(string userId)
        {
            var Result = await _userServices.DeleteStudent(userId);
            return Ok(Result);
        }
    }
}
