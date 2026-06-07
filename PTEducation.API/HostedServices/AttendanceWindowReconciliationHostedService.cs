using PTEducation.API.Realtime;
using PTEducation.Business.Services.AttendanceServices;
using PTEducation.Business.Services.ClassServices;
using PTEducation.Data.Entities;
using PTEducation.Data.Enums;
using PTEducation.Data.Repositories.AttendanceRepositories;

namespace PTEducation.API.HostedServices
{
    public class AttendanceWindowReconciliationHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AttendanceWindowReconciliationHostedService> _logger;

        public AttendanceWindowReconciliationHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<AttendanceWindowReconciliationHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await ReconcileAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task ReconcileAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var attendanceRepositories = scope.ServiceProvider.GetRequiredService<IAttendanceRepositories>();
                var attendanceServices = scope.ServiceProvider.GetRequiredService<IAttendanceServices>();
                var attendanceScheduler = scope.ServiceProvider.GetRequiredService<IAttendanceScheduler>();
                var classServices = scope.ServiceProvider.GetRequiredService<IClassServices>();
                var realtimeNotifier = scope.ServiceProvider.GetRequiredService<IAttendanceRealtimeNotifier>();

                var attendances = await attendanceRepositories.GetList(
                    x => x.Status != GeneralStatusEnums.Inactive.ToString());

                var now = DateTime.Now;
                foreach (var attendance in attendances)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var desiredStatus = ResolveAttendanceStatus(attendance, now);
                    if (string.Equals(desiredStatus, AttendanceStatusEnums.Closed.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        await attendanceServices.CloseAttendance(attendance.Id);
                        await BroadcastAttendanceStateAsync(attendance.ClassId, now, classServices, realtimeNotifier);
                        continue;
                    }

                    if (!string.Equals(attendance.Status, desiredStatus, StringComparison.OrdinalIgnoreCase))
                    {
                        attendance.Status = desiredStatus;
                        await attendanceRepositories.Update(attendance);
                    }

                    await attendanceScheduler.ScheduleAttendanceJobsAsync(attendance);

                    await BroadcastAttendanceStateAsync(attendance.ClassId, now, classServices, realtimeNotifier);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconcile attendance windows.");
            }
        }

        private static string ResolveAttendanceStatus(Attendance attendance, DateTime now)
        {
            var opensAt = DateTime.SpecifyKind(attendance.Date.ToDateTime(attendance.StartTime), DateTimeKind.Local);
            var closesAt = DateTime.SpecifyKind(attendance.Date.ToDateTime(attendance.EndTime), DateTimeKind.Local);

            if (now >= closesAt)
            {
                return AttendanceStatusEnums.Closed.ToString();
            }

            if (now >= opensAt)
            {
                return AttendanceStatusEnums.Opening.ToString();
            }

            return AttendanceStatusEnums.Pending.ToString();
        }

        private static async Task BroadcastAttendanceStateAsync(
            Guid classId,
            DateTime now,
            IClassServices classServices,
            IAttendanceRealtimeNotifier realtimeNotifier)
        {
            var metadata = await classServices.GetClassMetadata(classId);
            var nextSession = metadata.Data?.NextSession;
            var nextSessionEndAt = metadata.Data?.NextSessionEndAt;
            var windowKind = metadata.Data?.NextSessionKind;
            var isOpen = string.Equals(windowKind, "Current", StringComparison.OrdinalIgnoreCase) &&
                nextSession.HasValue &&
                nextSessionEndAt.HasValue &&
                now >= nextSession.Value &&
                now <= nextSessionEndAt.Value;

            await realtimeNotifier.BroadcastAttendanceWindowAsync(new AttendanceWindowStateDto
            {
                ClassId = classId,
                IsOpen = isOpen,
                WindowKind = windowKind,
                OpensAt = nextSession,
                ClosesAt = nextSessionEndAt,
                ServerTime = now,
                Reason = "Attendance window reconciled"
            });
        }
    }
}