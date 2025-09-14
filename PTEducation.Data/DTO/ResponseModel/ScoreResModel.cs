using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTEducation.Data.DTO.ResponseModel
{
    public class ScoreResModel
    {
    }

    public class ScoreDetailResModel
    {
        public Guid Id { get; set; }
        public DateTime TestDateAt { get; set; }
        public string ClassName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public ScoreCreatedByModel CreateBy { get; set; } = null!;
        public List<ScoreDetailStudentResModel>? ScoreDetails { get; set; } = new();
    }

    public class ScoreCreatedByModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class ScoreDetailStudentResModel
    {
        public Guid StudentClassId { get; set; }
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Score { get; set; }
        public string? Note { get; set; }
    }

    public class ScoreListResModel
    {
        public Guid Id { get; set; }
        public DateTime TestDateAt { get; set; }
        public ScoreCreatedByModel CreateBy { get; set; } = null!;
        public decimal AverageScore { get; set; }
        public string Status { get; set; } = null!;
    }

    public class ScoreStudentResModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<ScoreStudentDetailResModel> Scores { get; set; } = new();
    }

    public class ScoreStudentDetailResModel
    {
        public DateTime TestDateAt { get; set; }
        public string? Shift { get; set; }
        public decimal Score { get; set; }
        public string? Note { get; set; }
    }

    public class ScoreMonthResModel
    {
        public string Id { get; set; } = null!;
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
