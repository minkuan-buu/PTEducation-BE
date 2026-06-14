using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class ClassSchedule
{
    public Guid Id { get; set; }

    public Guid ClassId { get; set; }

    public byte DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Class Class { get; set; } = null!;
}
