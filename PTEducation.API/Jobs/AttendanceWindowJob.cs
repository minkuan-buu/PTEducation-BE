using Quartz;
using PTEducation.Data.Repositories.AttendanceRepositories;
using PTEducation.API.Realtime;
using PTEducation.Data.Enums;
using PTEducation.API.Realtime;
using PTEducation.Business.Services.ClassServices;

namespace PTEducation.API.Jobs
{
    public class AttendanceWindowJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IAttendanceRealtimeNotifier _notifier;
        private readonly IClassServices _classServices;

        public AttendanceWindowJob(IServiceScopeFactory scopeFactory, IAttendanceRealtimeNotifier notifier, IClassServices classServices)
        {
            _scopeFactory = scopeFactory;
            _notifier = notifier;
            _classServices = classServices;
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
                if (!attendance.Status.Equals(AttendanceStatusEnums.Closed.ToString()))
                {
                    attendance.Status = AttendanceStatusEnums.Closed.ToString();
                    await attendanceRepo.Update(attendance);
                }

                var metadata = await _classServices.GetClassMetadata(attendance.ClassId);
                var nextSession = metadata.Data?.NextSession;

                if (!nextSession.HasValue || nextSession.Value == default)
                {
                    await _notifier.BroadcastAttendanceWindowAsync(new AttendanceWindowStateDto
                    {
                        ClassId = attendance.ClassId,
                        IsOpen = false,
                        OpensAt = null,
                        ClosesAt = closesAt,
                        ServerTime = DateTime.Now,
                        Reason = "No upcoming attendance"
                    });
                    return;
                }

                await _notifier.BroadcastAttendanceWindowAsync(new AttendanceWindowStateDto
                {
                    ClassId = attendance.ClassId,
                    IsOpen = false,
                    OpensAt = nextSession,
                    ClosesAt = null,
                    ServerTime = DateTime.Now,
                    Reason = "Next session refreshed"
                });

                return;
            }

            await _notifier.BroadcastAttendanceWindowAsync(new AttendanceWindowStateDto
            {
                ClassId = attendance.ClassId,
                IsOpen = attendance.Status.Equals(AttendanceStatusEnums.Opening.ToString()),
                OpensAt = opensAt,
                ClosesAt = closesAt,
                ServerTime = DateTime.Now,
                Reason = action.Equals("open", StringComparison.OrdinalIgnoreCase) ? "Attendance window opened" : "Attendance window closed"
            });
        }
    }
}
