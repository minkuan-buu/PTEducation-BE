using Quartz;
using PTEducation.Data.Repositories.AttendanceRepositories;
using PTEducation.API.Realtime;
using PTEducation.Data.Enums;
using PTEducation.Business.Services.ClassServices;
using PTEducation.Business.Services.AttendanceServices;

namespace PTEducation.API.Jobs
{
    public class AttendanceWindowJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAttendanceRealtimeNotifier _notifier;
        private readonly IClassServices _classServices;
        private readonly IAttendanceServices _attendanceServices;

        public AttendanceWindowJob(IServiceScopeFactory scopeFactory, IAttendanceRealtimeNotifier notifier, IClassServices classServices, IAttendanceServices attendanceServices)
        {
            _scopeFactory = scopeFactory;
            _notifier = notifier;
            _classServices = classServices;
            _attendanceServices = attendanceServices;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var data = context.MergedJobDataMap;
            var attendanceIdStr = data.GetString("AttendanceId");
            var action = data.GetString("Action"); // "open" or "close"
            if (string.IsNullOrEmpty(attendanceIdStr) || string.IsNullOrEmpty(action)) return;
            if (!Guid.TryParse(attendanceIdStr, out var attendanceId)) return;

            using var scope = _scopeFactory.CreateScope();
            var attendanceRepo = scope.ServiceProvider.GetRequiredService<IAttendanceRepositories>();
            var attendance = await attendanceRepo.GetSingle(x => x.Id == attendanceId);
            if (attendance == null) return;

            var opensAt = DateTime.SpecifyKind(attendance.Date.ToDateTime(attendance.StartTime), DateTimeKind.Local);
            var closesAt = DateTime.SpecifyKind(attendance.Date.ToDateTime(attendance.EndTime), DateTimeKind.Local);

            if (action.Equals("open", StringComparison.OrdinalIgnoreCase))
            {
                if (!attendance.Status.Equals(AttendanceStatusEnums.Opening.ToString()))
                {
                    attendance.Status = AttendanceStatusEnums.Opening.ToString();
                    await attendanceRepo.Update(attendance);
                }
            }
            else if (action.Equals("close", StringComparison.OrdinalIgnoreCase))
            {
                await _attendanceServices.CloseAttendance(attendanceId);

                var metadata = await _classServices.GetClassMetadata(attendance.ClassId);
                var nextSession = metadata.Data?.NextSession;
                var nextSessionEndAt = metadata.Data?.NextSessionEndAt;
                var windowKind = metadata.Data?.NextSessionKind;
                var serverTime = DateTime.Now;
                var isOpen = string.Equals(windowKind, "Current", StringComparison.OrdinalIgnoreCase) &&
                    nextSession.HasValue &&
                    nextSessionEndAt.HasValue &&
                    serverTime >= nextSession.Value &&
                    serverTime <= nextSessionEndAt.Value;

                await _notifier.BroadcastAttendanceWindowAsync(new AttendanceWindowStateDto
                {
                    ClassId = attendance.ClassId,
                    IsOpen = isOpen,
                    WindowKind = windowKind,
                    OpensAt = nextSession,
                    ClosesAt = nextSessionEndAt,
                    ServerTime = serverTime,
                    Reason = "Next session refreshed"
                });

                return;
            }

            await _notifier.BroadcastAttendanceWindowAsync(new AttendanceWindowStateDto
            {
                ClassId = attendance.ClassId,
                IsOpen = attendance.Status.Equals(AttendanceStatusEnums.Opening.ToString()),
                WindowKind = attendance.Status.Equals(AttendanceStatusEnums.Opening.ToString()) ? "Current" : "Upcoming",
                OpensAt = opensAt,
                ClosesAt = closesAt,
                ServerTime = DateTime.Now,
                Reason = action.Equals("open", StringComparison.OrdinalIgnoreCase) ? "Attendance window opened" : "Attendance window closed"
            });
        }
    }
}
