using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class StudentClass
{
    public Guid Id { get; set; }

    public Guid ClassId { get; set; }

    public string StudentId { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual ICollection<AttendanceDetail> AttendanceDetails { get; set; } = new List<AttendanceDetail>();

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<ScoreDetail> ScoreDetails { get; set; } = new List<ScoreDetail>();

    public virtual User Student { get; set; } = null!;

    public virtual ICollection<StudentTuition> StudentTuitions { get; set; } = new List<StudentTuition>();
}
