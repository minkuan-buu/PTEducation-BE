using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class Attendance
{
    public Guid Id { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public Guid ClassId { get; set; }

    public string CreatedBy { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual ICollection<AttendanceDetail> AttendanceDetails { get; set; } = new List<AttendanceDetail>();

    public virtual Class Class { get; set; } = null!;

    public virtual User CreatedByNavigation { get; set; } = null!;
}
