using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class StudentTuition
{
    public Guid Id { get; set; }

    public Guid TuitionPeriodId { get; set; }

    public Guid StudentClassId { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public DateTime? PaidAt { get; set; }

    public string? Note { get; set; }

    public virtual StudentClass StudentClass { get; set; } = null!;

    public virtual TuitionPeriod TuitionPeriod { get; set; } = null!;
}
