namespace PTEducation.Data.DTO.ResponseModel
{
    public class StudentGuardianOverviewResModel
    {
        public string ClassName { get; set; } = null!;
        public decimal AverageScore { get; set; }
        public decimal AttendanceRate { get; set; }
        public DateTime? NextSession { get; set; } = null;
        public List<AttendanceSessionsResModel>? RecentAttendances { get; set; } = null;
        public List<ScoreSessionResModel>? RecentScores { get; set; } = null;
    }

    public class AttendanceSessionsResModel
    {
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string SessionType { get; set; } = null!;
        public string Note { get; set; } = null!;
        public string AttendanceStatus { get; set; } = null!;
        public string Status { get; set; } = null!;
    }

    public class ScoreSessionResModel
    {
        public DateTime TestDateAt { get; set; }
        public string Shift { get; set; } = null!;
        public string Score { get; set; } = null!;
        public string Note { get; set; } = null!;
    }
}