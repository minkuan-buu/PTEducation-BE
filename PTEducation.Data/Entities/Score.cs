using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class Score
{
    public Guid Id { get; set; }

    public DateTime TestDateAt { get; set; }

    public Guid ClassId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string Status { get; set; } = null!;

    public string? Shift { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<ScoreDetail> ScoreDetails { get; set; } = new List<ScoreDetail>();
}
