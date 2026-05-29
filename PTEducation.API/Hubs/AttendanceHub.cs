using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PTEducation.API.Realtime;

namespace PTEducation.API.Hubs
{
    [Authorize(AuthenticationSchemes = "PTEducationAuthentication")]
    public class AttendanceHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync(AttendanceRealtimeEvents.ServerTimeSynced, new
            {
                serverTime = DateTime.UtcNow,
                offsetMinutes = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMinutes
            });

            await base.OnConnectedAsync();
        }
    }
}
