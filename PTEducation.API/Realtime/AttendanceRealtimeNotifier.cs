using Microsoft.AspNetCore.SignalR;
using PTEducation.API.Hubs;

namespace PTEducation.API.Realtime
{
    public sealed class AttendanceRealtimeNotifier : IAttendanceRealtimeNotifier
    {
        private readonly IHubContext<AttendanceHub> _hubContext;

        public AttendanceRealtimeNotifier(IHubContext<AttendanceHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task BroadcastAttendanceWindowAsync(AttendanceWindowStateDto payload, CancellationToken cancellationToken = default)
        {
            return _hubContext.Clients.Group(AttendanceHub.GetClassGroupName(payload.ClassId))
                .SendAsync(AttendanceRealtimeEvents.AttendanceWindowStateChanged, payload, cancellationToken);
        }

        public Task BroadcastServerTimeAsync(DateTime serverTime, CancellationToken cancellationToken = default)
        {
            return _hubContext.Clients.All.SendAsync(AttendanceRealtimeEvents.ServerTimeSynced, new
            {
                serverTime,
                offsetMinutes = TimeZoneInfo.Local.GetUtcOffset(serverTime).TotalMinutes
            }, cancellationToken);
        }
    }
}
