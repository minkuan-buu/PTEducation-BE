using PTEducation.Data.DTO.Custom;
using PTEducation.Data.DTO.RequestModel;
using PTEducation.Business.Services.UserServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using PTEducation.Business.Services.ClassServices;
using PTEducation.Data.DTO.ResponseModel;
using PTEducation.Business.Services.AttendanceServices;
using Asp.Versioning;
using PTEducation.API.Realtime;

namespace PTEducation.API.Controllers
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/attendance")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceServices _attendanceServices;
        private readonly IAttendanceRealtimeNotifier _attendanceRealtimeNotifier;

        public AttendanceController(IAttendanceServices attendanceServices, IAttendanceRealtimeNotifier attendanceRealtimeNotifier)
        {
            _attendanceServices = attendanceServices;
            _attendanceRealtimeNotifier = attendanceRealtimeNotifier;
        }

        private static DateTime? CombineDateAndTime(DateTime date, TimeOnly? time)
        {
            if (!time.HasValue)
            {
                return null;
            }

            return date.Date.Add(time.Value.ToTimeSpan());
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

        // [HttpGet("all")]
        // [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        // public async Task<IActionResult> GetList(int? pageIndex, [FromQuery] AttendanceFilter searchModel)
        // {
        //     try
        //     {
        //         var Result = await _attendanceServices.GetListAttendance(pageIndex, searchModel);
        //         return Ok(Result);
        //     }
        //     catch (CustomException ex)
        //     {
        //         return BadRequest(new { message = ex.Message });
        //     }
        // }

        [HttpPost("create")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateAttendance([FromBody] AttendanceCreateReqModel AttendanceReq)
        {
            try
            {
                string token = Request.Headers["Authorization"].ToString().Split(" ")[1];
                var Result = await _attendanceServices.CreateAttendance(AttendanceReq, token);

                var opensAt = CombineDateAndTime(AttendanceReq.Date, AttendanceReq.StartTime);
                var closesAt = CombineDateAndTime(AttendanceReq.Date, AttendanceReq.EndTime);
                var serverTime = DateTime.UtcNow;
                var isOpen = opensAt.HasValue && serverTime >= opensAt.Value && (!closesAt.HasValue || serverTime <= closesAt.Value);

                await _attendanceRealtimeNotifier.BroadcastAttendanceWindowAsync(new AttendanceWindowStateDto
                {
                    ClassId = AttendanceReq.ClassId,
                    IsOpen = isOpen,
                    OpensAt = opensAt,
                    ClosesAt = closesAt,
                    ServerTime = serverTime,
                    Reason = isOpen ? "Attendance window opened" : "Attendance window scheduled"
                });

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
