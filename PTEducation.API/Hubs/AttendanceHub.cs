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

        public async Task JoinClassGroup(string classId)
        {
            if (!Guid.TryParse(classId, out var parsedClassId))
            {
                throw new HubException("Invalid class id.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GetClassGroupName(parsedClassId));

            var metadata = await _classServices.GetClassMetadata(parsedClassId);
            var nextSession = metadata.Data?.NextSession;

            if (nextSession.HasValue && nextSession.Value != default)
            {
                await Clients.Caller.SendAsync(AttendanceRealtimeEvents.AttendanceWindowStateChanged, new AttendanceWindowStateDto
                {
                    ClassId = parsedClassId,
                    IsOpen = DateTime.UtcNow >= nextSession.Value.ToUniversalTime(),
                    OpensAt = nextSession.Value,
                    ClosesAt = null,
                    ServerTime = DateTime.UtcNow,
                    Reason = "Initial class window state"
                });
            }
        }

        public async Task<AttendanceWindowStateDto?> GetClassWindowState(string classId)
        {
            if (!Guid.TryParse(classId, out var parsedClassId))
            {
                throw new HubException("Invalid class id.");
            }

            var metadata = await _classServices.GetClassMetadata(parsedClassId);
            var nextSession = metadata.Data?.NextSession;

            if (!nextSession.HasValue || nextSession.Value == default)
            {
                return null;
            }

            return new AttendanceWindowStateDto
            {
                ClassId = parsedClassId,
                IsOpen = DateTime.UtcNow >= nextSession.Value.ToUniversalTime(),
                OpensAt = nextSession.Value,
                ClosesAt = null,
                ServerTime = DateTime.UtcNow,
                Reason = "Initial class window state"
            };
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
