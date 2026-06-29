using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class TuitionPeriod
{
    public Guid Id { get; set; }

    public int GradeId { get; set; }

    public string Title { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime? DueDate { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Grade Grade { get; set; } = null!;

    public virtual ICollection<StudentTuition> StudentTuitions { get; set; } = new List<StudentTuition>();
}
