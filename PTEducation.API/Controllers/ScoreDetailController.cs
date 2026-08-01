using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Business.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using PTEducation.Business.Services.ClassServices;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Business.Services.ScoreDetailServices;
using PTEducation.Business.Services.StudentServices;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/score-detail")]
    public class ScoreDetailController : ControllerBase
    {
        private readonly IScoreDetailServices _scoreDetailServices;
        private readonly IStudentServices _studentServices;
        public ScoreDetailController(IScoreDetailServices scoreDetailServices, IStudentServices studentServices)
        {
            _studentServices = studentServices;
            _scoreDetailServices = scoreDetailServices;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Student,Guardian")]
        public async Task<IActionResult> GetScoreByMonth([FromQuery] int Month, [FromQuery] int Year, [FromQuery] string? classId)
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Unauthorized access." });
                }
                var Result = await _studentServices.GetScoreByMonth(Month, Year, userId, classId);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("month")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Student,Guardian")]
        public async Task<IActionResult> GetScoreMonth([FromQuery] string? classId)
        {
            try
            {
                var userId = User.FindFirst("userid")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Unauthorized access." });
                }
                var Result = await _studentServices.GetScoreMonth(userId, classId);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateScore([FromBody] ScoreDetailUpdateReqModel ScoreReq)
        {
            try
            {
                var Result = await _scoreDetailServices.UpdateScore(ScoreReq);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
