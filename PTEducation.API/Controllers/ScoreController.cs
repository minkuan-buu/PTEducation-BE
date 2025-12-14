using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Business.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using PTEducation.Business.Services.ClassServices;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Business.Services.ScoreServices;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/score")]
    public class ScoreController : ControllerBase
    {
        private readonly IScoreServices _scoreServices;
        public ScoreController(IScoreServices scoreServices)
        {
            _scoreServices = scoreServices;
        }

        [HttpGet("get")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetScoreDetail(Guid Id)
        {
            try
            {
                var Result = await _scoreServices.GetScoreDetail(Id);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetList(int? pageIndex, [FromQuery] ScoreFilter searchModel)
        {
            try
            {
                var Result = await _scoreServices.GetListScore(pageIndex, searchModel);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("create")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateScore([FromBody] ScoreCreateReqModel ScoreReq)
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var Result = await _scoreServices.CreateScore(ScoreReq, token);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("sheet/create")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateScoreFromSheet([FromBody] ScoreCreateReqModel ScoreReq)
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var Result = await _scoreServices.CreateScoreFromSheet(ScoreReq, token);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet()]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetScoreIdByDateAndClassId([FromQuery] ScoreIdReqModel scoreIdReq)
        {
            try
            {
                // string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var Result = await _scoreServices.GetScoreIdByDateAndClassId(scoreIdReq);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateScore([FromBody] ScoreUpdateReqModel ScoreReq)
        {
            try
            {
                var Result = await _scoreServices.UpdateScore(ScoreReq);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("delete")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> SoftDeleteClass([FromBody] Guid Id)
        {
            try
            {
                var Result = await _scoreServices.SoftDeleteScore(Id);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("restore")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> RestoreScore([FromBody] Guid Id)
        {
            try
            {
                var Result = await _scoreServices.RestoreScore(Id);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("delete/{Id}")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> HardDeleteScore(Guid Id)
        {
            var Result = await _scoreServices.HardDeleteScore(Id);
            return Ok(Result);
        }
    }
}
