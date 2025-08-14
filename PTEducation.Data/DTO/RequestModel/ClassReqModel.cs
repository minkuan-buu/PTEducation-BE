using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
