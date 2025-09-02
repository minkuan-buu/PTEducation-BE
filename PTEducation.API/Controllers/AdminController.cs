using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Business.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTEducation.Business.Services.AuthServices;
using Org.BouncyCastle.Ocsp;
using PTEducation.Data.DTO.ResponseModel;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserServices _userServices;
        public AdminController(IUserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpGet("managers")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetManagers(int? pageIndex, [FromQuery] UserFilter searchModel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var Result = await _userServices.GetManagers(pageIndex, searchModel, token);
            return Ok(Result);
        }

        [HttpPost("managers")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> RegisterManager([FromBody] List<ManagerRegisterReqModel> ReqModel)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var Result = await _userServices.Register(ReqModel);
            return Ok(Result);
        }

        [HttpPost("manager/deactivate/{id}")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> DeactivateManager(string id)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var Result = await _userServices.Deactivate(id);
            return Ok(Result);
        }

        [HttpPost("manager/reactivate/{id}")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> ReactivateManager(string id)
        {
            string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
            var Result = await _userServices.ReActivate(id);
            return Ok(Result);
        }

        [HttpPut("student/{studentClassId}")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateStudent(Guid studentClassId, [FromBody] StudentUpdateReqModel reqModel)
        {
            var Result = await _userServices.UpdateStudentInfo(reqModel, studentClassId);
            return Ok(Result);
        }

        [HttpDelete("student/{studentClassId}")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteStudent(Guid studentClassId)
        {
            var Result = await _userServices.DeleteStudent(studentClassId);
            return Ok(Result);
        }
    }
}
