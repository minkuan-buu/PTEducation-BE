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
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid ClassId { get; set; }
        public List<string> ListIdStudent { get; set; } = new();
    }

    public class AttendanceUpdateReqModel
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class AttendanceFilter
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid ClassId { get; set; }
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
}
