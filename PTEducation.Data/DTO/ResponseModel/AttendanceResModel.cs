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
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AttendanceCreatedByModel CreatedBy { get; set; } = null!;
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public string Status { get; set; } = null!;
    }

    public class AttendanceCreatedByModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class AttendanceDetailResModel
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ClassName { get; set; } = null!;
        public AttendanceCreatedByModel CreatedBy { get; set; } = null!;
        public List<AttendanceDetailStudentResModel>? AttendanceDetails { get; set; } = new();
        public string Status { get; set; } = null!;
    }

    public class AttendanceDetailStudentResModel
    {
        public Guid StudentClassId { get; set; }
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string AttendanceStatus { get; set; } = null!;
    }

    public class AttendanceStudentResModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<AttendanceStudentDetailResModel> Attendances { get; set; } = new();
    }

    public class AttendanceStudentDetailResModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool isPresent { get; set; }
    }

    public class AttendanceMonthResModel
    {
        public string Id { get; set; } = null!;
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
