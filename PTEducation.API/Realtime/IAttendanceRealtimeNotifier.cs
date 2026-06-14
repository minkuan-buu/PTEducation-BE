namespace PTEducation.API.Realtime
{
    public interface IAttendanceRealtimeNotifier
    {
        Task BroadcastAttendanceWindowAsync(AttendanceWindowStateDto payload, CancellationToken cancellationToken = default);
        Task BroadcastServerTimeAsync(DateTime serverTime, CancellationToken cancellationToken = default);
    }
}
