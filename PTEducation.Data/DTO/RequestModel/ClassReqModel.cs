using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PTEducation.Data.Entities;

namespace PTEducation.Data.DTO.RequestModel
{
    public class ClassReqModel
    {
    }

    public class ClassCreateReqModel
    {
        public string Name { get; set; } = null!;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string? DefaultPassword { get; set; }
        public List<StudentsImportWithClass>? Students { get; set; } = new();
    }

    public class ClassCreateReqModelV2
    {
        public string Name { get; set; } = null!;
        public List<ClassScheduleReqModel> Schedule { get; set; } = null!;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
    }

    public class ClassScheduleReqModel
    {
        public byte DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }

    public class ClassUpdateReqModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
    }

    public class StudentsImportWithClass
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
    }

    public class ManualAddStudentClassModel
    {
        public Guid Id { get; set; }
        public string? DefaultPassword { get; set; }
        public List<StudentsImportWithClass> Students { get; set; } = new();
    }

    public class MoveOutStudentClassModel
    {
        public Guid StudentId { get; set; }
        public Guid TargetClassId { get; set; }
    }
}
