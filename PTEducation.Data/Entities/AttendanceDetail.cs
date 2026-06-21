using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class AttendanceDetail
{
    public Guid Id { get; set; }

    public Guid AttendanceId { get; set; }

    public Guid StudentClassId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public Guid? MakeUpSession { get; set; }

    public virtual Attendance Attendance { get; set; } = null!;

    public virtual Attendance? MakeUpSessionNavigation { get; set; }

    public virtual StudentClass StudentClass { get; set; } = null!;
}
