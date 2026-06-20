using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class Attendance
{
    public Guid Id { get; set; }

    public Guid ClassId { get; set; }

    public string Status { get; set; } = null!;

    public Guid? ClassScheduleId { get; set; }

    public string SessionType { get; set; } = null!;

    public string? Note { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public DateOnly Date { get; set; }

    public virtual ICollection<AttendanceDetail> AttendanceDetailAttendances { get; set; } = new List<AttendanceDetail>();

    public virtual ICollection<AttendanceDetail> AttendanceDetailMakeUpSessionNavigations { get; set; } = new List<AttendanceDetail>();

    public virtual Class Class { get; set; } = null!;

    public virtual ClassSchedule? ClassSchedule { get; set; }
}
