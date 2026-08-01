using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTEducation.Business.Services.OverviewServices;
using PTEducation.Data.DTO.Custom;
using System.Threading.Tasks;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/overview")]
    [ApiVersion("2.0")]
    public class OverviewController : ControllerBase
    {
        private readonly IOverviewServices _overviewServices;

        public OverviewController(IOverviewServices overviewServices)
        {
            _overviewServices = overviewServices;
        }

        [HttpGet("classes")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Student,Guardian")]
        public async Task<IActionResult> GetStudentClasses()
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Unauthorized access." });
                }

                var result = await _overviewServices.GetStudentClasses(userId);
                return Ok(result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("student")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Student,Guardian")]
        public async Task<IActionResult> GetOverviewForStudentOrGuardian([FromQuery] string? classId)
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Unauthorized access." });
                }

                var result = await _overviewServices.GetOverviewForStudentOrGuardian(userId, classId);
                return Ok(result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("attendance")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Student,Guardian")]
        public async Task<IActionResult> GetAttendanceOverviewForStudentOrGuardian([FromQuery] string? classId)
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Unauthorized access." });
                }

                var result = await _overviewServices.GetAttendanceOverviewForStudentOrGuardian(userId, classId);
                return Ok(result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
