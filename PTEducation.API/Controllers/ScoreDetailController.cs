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
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Student")]
        public async Task<IActionResult> GetScoreStudentByMonth([FromQuery] ScoreStudentReqModel ScoreReq)
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var Result = await _studentServices.GetScoreByMonth(ScoreReq.Month, ScoreReq.Year, token);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("month")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Student")]
        public async Task<IActionResult> GetMonthTest()
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var Result = await _studentServices.GetScoreMonth(token);
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
