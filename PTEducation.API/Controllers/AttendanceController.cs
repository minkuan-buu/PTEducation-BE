using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Business.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using PTEducation.Business.Services.ClassServices;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Business.Services.AttendanceServices;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/attendance")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceServices _attendanceServices;
        public AttendanceController(IAttendanceServices attendanceServices)
        {
            _attendanceServices = attendanceServices;
        }

        [HttpGet("get")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAttendanceDetail(Guid Id)
        {
            try
            {
                var Result = await _attendanceServices.GetAttendanceDetail(Id);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("all")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetList(int? pageIndex, [FromQuery] AttendanceFilter searchModel)
        {
            try
            {
                var Result = await _attendanceServices.GetListAttendance(pageIndex, searchModel);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("create")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateAttendance([FromBody] AttendanceCreateReqModel AttendanceReq)
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var Result = await _attendanceServices.CreateAttendance(AttendanceReq, token);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateAttendance([FromBody] AttendanceUpdateReqModel AttendanceReq)
        {
            try
            {
                var Result = await _attendanceServices.UpdateAttendance(AttendanceReq);
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
                var Result = await _attendanceServices.SoftDeleteAttendance(Id);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("restore")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> RestoreAttendance([FromBody] Guid Id)
        {
            try
            {
                var Result = await _attendanceServices.RestoreAttendance(Id);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
