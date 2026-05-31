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
    [Route("api/v{version:apiVersion}/attendances")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceServices _attendanceServices;
        private readonly IAttendanceRealtimeNotifier _attendanceRealtimeNotifier;

        public AttendanceController(IAttendanceServices attendanceServices, IAttendanceRealtimeNotifier attendanceRealtimeNotifier)
        {
            _attendanceServices = attendanceServices;
            _attendanceRealtimeNotifier = attendanceRealtimeNotifier;
        }

        private static AttendanceWindowStateDto BuildWindowState(
            AttendanceMutationResModel attendance,
            string? reason = null,
            bool? isOpenOverride = null)
        {
            var opensAt = attendance.Date.ToDateTime(attendance.StartTime);
            var closesAt = attendance.Date.ToDateTime(attendance.EndTime);
            var serverTime = DateTime.UtcNow;
            var isOpen = isOpenOverride ?? (serverTime >= opensAt && serverTime <= closesAt);

            return new AttendanceWindowStateDto
            {
                ClassId = attendance.ClassId,
                IsOpen = isOpen,
                OpensAt = opensAt,
                ClosesAt = closesAt,
                ServerTime = serverTime,
                Reason = reason
            };
        }

        [HttpGet("classes/{classId:guid}")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAttendanceSessions(Guid classId, [FromQuery] DateTime date)
        {
            try
            {
                var Result = await _attendanceServices.GetAttendanceSessions(classId, DateOnly.FromDateTime(date));
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

        [HttpPost("classes/{classId:guid}")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateAttendance(Guid classId, [FromBody] AttendanceCreateReqModel AttendanceReq)
        {
            try
            {
                var Result = await _attendanceServices.CreateAttendance(AttendanceReq, classId);

                await _attendanceRealtimeNotifier.BroadcastAttendanceWindowAsync(
                    BuildWindowState(Result, "Attendance window scheduled"));

                // scheduling handled in service layer

                return Ok(new MessageResultModel { Message = "Ok" });
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

                await _attendanceRealtimeNotifier.BroadcastAttendanceWindowAsync(
                    BuildWindowState(Result, "Attendance window updated"));

                // scheduling handled in service layer

                return Ok(new MessageResultModel { Message = "Ok" });
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

                await _attendanceRealtimeNotifier.BroadcastAttendanceWindowAsync(
                    BuildWindowState(Result, "Attendance window deleted", false));

                return Ok(new MessageResultModel { Message = "Ok" });
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

                await _attendanceRealtimeNotifier.BroadcastAttendanceWindowAsync(
                    BuildWindowState(Result, "Attendance window restored"));

                return Ok(new MessageResultModel { Message = "Ok" });
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
