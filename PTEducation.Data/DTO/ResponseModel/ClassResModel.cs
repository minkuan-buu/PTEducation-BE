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
        public int TotalStudent { get; set; }
        public string Status { get; set; } = null!;
    }

    public class ClassCreatedByModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class StudentClassModel
    {
        public Guid Id { get; set; }
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
}
