using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.DTO.ResponseModel
{
    public class ClassResModel
    {
    }

    public class ClassDetailResModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public ClassCreatedByModel CreatedBy { get; set; } = null!;
        public List<StudentClassModel>? Students { get; set; } = new();
    }

    public class ClassListSelectResModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class ListClassResModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public ClassCreatedByModel CreatedBy { get; set; } = null!;
        public List<ClassScheduleResModel> WeeklySchedules { get; set; } = new();
        public int TotalStudent { get; set; }
        public string Status { get; set; } = null!;
    }

    public class ClassCreatedByModel
    {
        public String Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class StudentClassModel
    {
        public Guid Id { get; set; }
        public string StudentCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
    }

    public class ClassFilter
    {
        public string? Keyword { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public bool? OrderCreatedAt { get; set; }
    }

    public class ClassDetailMetaData
    {
        public string Name { get; set; } = null!;
        public int TotalStudent { get; set; }
        public int TotalPendingStudent { get; set; }
        public decimal AverageScore { get; set; }
        public decimal AttendanceRate { get; set; }
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public List<ClassScheduleResModel> WeeklySchedules { get; set; } = new();
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public DateTime? NextSession { get; set; }
        public DateTime? NextSessionEndAt { get; set; }
        public string? NextSessionKind { get; set; }
    }

    public class ClassScheduleResModel
    {
        public byte DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }

    public class ClassPeersListResModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        
    }
}
