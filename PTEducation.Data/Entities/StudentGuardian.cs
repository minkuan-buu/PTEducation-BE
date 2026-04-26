using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class StudentGuardian
{
    public Guid Id { get; set; }

    public string StudentId { get; set; } = null!;

    public string GuardianId { get; set; } = null!;

    public string Relationship { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public virtual User Guardian { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
