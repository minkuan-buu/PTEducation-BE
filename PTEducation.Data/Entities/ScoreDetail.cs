using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class ScoreDetail
{
    public Guid Id { get; set; }

    public Guid ScoreId { get; set; }

    public Guid StudentClassId { get; set; }

    public decimal Score { get; set; }

    public string Status { get; set; } = null!;

    public virtual Score ScoreNavigation { get; set; } = null!;

    public virtual StudentClass StudentClass { get; set; } = null!;
}
