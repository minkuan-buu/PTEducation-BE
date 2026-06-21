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
        private readonly IClassServices _classServices;

        public AttendanceController(IAttendanceServices attendanceServices, IAttendanceRealtimeNotifier attendanceRealtimeNotifier, IClassServices classServices)
        {
            _attendanceServices = attendanceServices;
            _attendanceRealtimeNotifier = attendanceRealtimeNotifier;
            _classServices = classServices;
        }

        private async Task<AttendanceWindowStateDto> BuildWindowStateAsync(Guid classId, string? reason = null)
        {
            var metadata = await _classServices.GetClassMetadata(classId);
            var serverTime = DateTime.Now;
            var opensAt = metadata.Data?.NextSession;
            var closesAt = metadata.Data?.NextSessionEndAt;
            var windowKind = metadata.Data?.NextSessionKind;
            var isOpen = string.Equals(windowKind, "Current", StringComparison.OrdinalIgnoreCase) &&
                opensAt.HasValue &&
                closesAt.HasValue &&
                serverTime >= opensAt.Value &&
                serverTime <= closesAt.Value;

            return new AttendanceWindowStateDto
            {
                ClassId = classId,
                IsOpen = isOpen,
                WindowKind = windowKind,
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

        [HttpGet("{Id:guid}")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAttendanceDetail(Guid Id, [FromQuery] Guid? classId = null)
        {
            try
            {
                var Result = await _attendanceServices.GetAttendanceDetail(Id, classId);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("classes/{classId:guid}/students/{studentClassId}/absent-sessions")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> GetStudentAbsentSessions(Guid classId, Guid studentClassId)
        {
            try
            {
                var Result = await _attendanceServices.GetStudentAbsentSessions(classId, studentClassId);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{Id:guid}")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateAttendanceDetail(Guid Id, [FromBody] List<AttendanceDetailStudentReqModel> attendanceDetailReq)
        {
            try
            {
                var Result = await _attendanceServices.UpdateAttendanceV2(Id, attendanceDetailReq);
                return Ok(Result);
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("classes/{classId:guid}")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateAttendance(Guid classId, [FromBody] AttendanceCreateReqModel AttendanceReq)
        {
            try
            {
                var Result = await _attendanceServices.CreateAttendance(AttendanceReq, classId);

                await _attendanceRealtimeNotifier.BroadcastAttendanceWindowAsync(
                    await BuildWindowStateAsync(Result.ClassId, "Attendance window scheduled"));

                // scheduling handled in service layer

                return Ok(new MessageResultModel { Message = "Ok" });
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{attendanceId:guid}/check-attendance")]
        [Authorize(AuthenticationSchemes = "PTEducationAuthentication", Roles = "Admin,Manager")]
        public async Task<IActionResult> CheckAttendance(Guid attendanceId, [FromBody] CheckAttendanceReqModel checkAttendanceReq)
        {
            try
            {
                var Result = await _attendanceServices.CheckAttendance(attendanceId, checkAttendanceReq.StudentClassId);
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

                await _attendanceRealtimeNotifier.BroadcastAttendanceWindowAsync(
                    await BuildWindowStateAsync(Result.ClassId, "Attendance window updated"));

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
                    await BuildWindowStateAsync(Result.ClassId, "Attendance window deleted"));

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
                    await BuildWindowStateAsync(Result.ClassId, "Attendance window restored"));

                return Ok(new MessageResultModel { Message = "Ok" });
            }
            catch (CustomException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
