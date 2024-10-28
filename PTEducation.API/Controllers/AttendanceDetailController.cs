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
using PTEducation.Business.Services.AttendanceDetailServices;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [Route("api/attendance-detail")]
    public class AttendanceDetailController : ControllerBase
    {
        private readonly IAttendanceDetailServices _attendanceDetailServices;
        private readonly IStudentServices _studentServices;
        public AttendanceDetailController(IAttendanceDetailServices attendanceDetailServices, IStudentServices studentServices)
        {
            _studentServices = studentServices;
            _attendanceDetailServices = attendanceDetailServices;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Student")]
        public async Task<IActionResult> GetAttendanceStudentByMonth([FromQuery] AttendanceStudentReqModel AttendanceReq)
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var Result = await _studentServices.GetAttendanceByMonth(AttendanceReq.Month, AttendanceReq.Year, token);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("month")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Student")]
        public async Task<IActionResult> GetMonthAttendance()
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var Result = await _studentServices.GetAttendanceMonth(token);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateScore([FromBody] AttendanceDetailUpdateReqModel AttendanceReq)
        {
            try
            {
                var Result = await _attendanceDetailServices.UpdateAttendance(AttendanceReq);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
