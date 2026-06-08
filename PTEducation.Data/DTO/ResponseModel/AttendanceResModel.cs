using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.DTO.ResponseModel
{
    public class AttendanceResModel
    {
    }

    public class AttendanceListResModel
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public Guid? ClassScheduleId { get; set; }
        public string SessionType { get; set; } = null!;
        public string? Note { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public string Status { get; set; } = null!;
    }

    public class AttendanceDetailResModel
    {
        public AttendanceDetailSessionResModel Session { get; set; } = new();
        public List<AttendanceDetailStudentResModel> AttendanceDetails { get; set; } = new();
    }

    public class AttendanceDetailSessionResModel
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public Guid? ClassScheduleId { get; set; }
        public string SessionType { get; set; } = null!;
        public string? Note { get; set; }
        public string Status { get; set; } = null!;
    }

    public class AttendanceSessionResModel
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string SessionType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? Note { get; set; }
    }

    public class AttendanceMutationResModel
    {
        public Guid AttendanceId { get; set; }
        public Guid ClassId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string SessionType { get; set; } = null!;
        public string Status { get; set; } = null!;
    }

    public class AttendanceDetailStudentResModel
    {
        public Guid StudentClassId { get; set; }
        public string StudentId { get; set; } = null!;
        public string StudentName { get; set; } = null!;
        public string AttendanceStatus { get; set; } = null!;
        public List<UserGuardianListResModel> Guardians { get; set; } = null!;
    }

    public class AttendanceStudentResModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<AttendanceStudentDetailResModel> Attendances { get; set; } = new();
    }

    public class AttendanceStudentDetailResModel
    {
        public DateOnly Date { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public bool isPresent { get; set; }
    }

    public class AttendanceMonthResModel
    {
        public string Id { get; set; } = null!;
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
