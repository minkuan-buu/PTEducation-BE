using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class Otp
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Code { get; set; } = null!;

    public bool IsUsed { get; set; }

    public DateTime ExpiredDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
