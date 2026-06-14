using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PTEducation.API.Realtime;
using PTEducation.Business.Services.ClassServices;
using PTEducation.Data.DTO.ResponseModel;

namespace PTEducation.API.Hubs
{
    [Authorize(AuthenticationSchemes = "PTEducationAuthentication")]
    public class AttendanceHub : Hub
    {
        private readonly IClassServices _classServices;

        public AttendanceHub(IClassServices classServices)
        {
            _classServices = classServices;
        }

        public static string GetClassGroupName(Guid classId) => $"class:{classId:D}";

        private async Task<AttendanceWindowStateDto> BuildWindowStateAsync(Guid classId, string? reason = null)
        {
            var metadata = await _classServices.GetClassMetadata(classId);
            var serverTime = DateTime.UtcNow;
            var opensAt = metadata.Data?.NextSession;
            var closesAt = metadata.Data?.NextSessionEndAt;
            var windowKind = metadata.Data?.NextSessionKind;
            var isOpen = string.Equals(windowKind, "Current", StringComparison.OrdinalIgnoreCase) &&
                opensAt.HasValue &&
                closesAt.HasValue &&
                serverTime >= opensAt.Value.ToUniversalTime() &&
                serverTime <= closesAt.Value.ToUniversalTime();

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

        public async Task JoinClassGroup(string classId)
        {
            if (!Guid.TryParse(classId, out var parsedClassId))
            {
                throw new HubException("Invalid class id.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GetClassGroupName(parsedClassId));

            await Clients.Caller.SendAsync(
                AttendanceRealtimeEvents.AttendanceWindowStateChanged,
                await BuildWindowStateAsync(parsedClassId, "Initial class window state"));
        }

        public async Task<AttendanceWindowStateDto?> GetClassWindowState(string classId)
        {
            if (!Guid.TryParse(classId, out var parsedClassId))
            {
                throw new HubException("Invalid class id.");
            }

            return await BuildWindowStateAsync(parsedClassId, "Initial class window state");
        }

        public async Task LeaveClassGroup(string classId)
        {
            if (!Guid.TryParse(classId, out var parsedClassId))
            {
                throw new HubException("Invalid class id.");
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetClassGroupName(parsedClassId));
        }

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
