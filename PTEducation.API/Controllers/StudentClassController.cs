using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Business.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTEducation.Business.Services.AuthServices;
using PTEducation.Business.Services.StudentClassServices;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/student-class/")]
    public class StudentClassController : ControllerBase
    {
        private readonly IStudentClassServices _studentClassServices;
        public StudentClassController(IStudentClassServices studentClassServices)
        {
            _studentClassServices = studentClassServices;
        }

        [HttpGet("all")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetStudentInClassForSheet(Guid classId)
        {
            var Result = await _studentClassServices.GetStudentInClassForSheet(classId);
            return Ok(Result);
        }
    }
}
