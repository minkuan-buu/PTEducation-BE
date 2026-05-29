namespace PTEducation.API.Realtime
{
    public sealed record AttendanceWindowStateDto
    {
        public Guid ClassId { get; init; }
        public bool IsOpen { get; init; }
        public DateTime? OpensAt { get; init; }
        public DateTime? ClosesAt { get; init; }
        public DateTime ServerTime { get; init; }
        public string? Reason { get; init; }
    }
}
