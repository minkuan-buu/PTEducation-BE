using System;
using System.Collections.Generic;

namespace PTEducation.Data.Entities;

public partial class User
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Role { get; set; } = null!;

    public byte[] Password { get; set; } = null!;

    public byte[] Salt { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool IsNeedResetPassword { get; set; }

    public string? PasswordBcrypt { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Otp> Otps { get; set; } = new List<Otp>();

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();

    public virtual ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();

    public virtual ICollection<StudentGuardian> StudentGuardianGuardians { get; set; } = new List<StudentGuardian>();

    public virtual ICollection<StudentGuardian> StudentGuardianStudents { get; set; } = new List<StudentGuardian>();
}
