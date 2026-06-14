using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.DTO.RequestModel
{
    public class AttendanceReqModel
    {
    }

    public class AttendanceCreateReqModel
    {
        public DateTime Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string SessionType { get; set; } = "Adhoc";
        public string? Note { get; set; }
    }

    public class AttendanceUpdateReqModel
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public Guid? ClassScheduleId { get; set; }
        public string SessionType { get; set; } = "Fixed";
        public string? Note { get; set; }
    }

    public class AttendanceFilter
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class AttendanceDetailUpdateReqModel
    {
        public Guid Id { get; set; }
        public List<AttendanceDetailStudentReqModel> AttendanceReqList { get; set; } = new();
    }

    public class AttendanceDetailStudentReqModel
    {
        public Guid StudentClassId { get; set; }
        public string AttendanceStatus { get; set; } = null!;
    }

    public class AttendanceStudentReqModel
    {
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class CheckAttendanceReqModel
    {
        public Guid StudentClassId { get; set; }
    }
}
