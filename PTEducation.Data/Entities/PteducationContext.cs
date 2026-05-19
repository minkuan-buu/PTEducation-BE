using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PTEducation.Data.Entities;

public partial class PteducationContext : DbContext
{
    public PteducationContext(DbContextOptions<PteducationContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<AttendanceDetail> AttendanceDetails { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassSchedule> ClassSchedules { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    public virtual DbSet<Score> Scores { get; set; }

    public virtual DbSet<ScoreDetail> ScoreDetails { get; set; }

    public virtual DbSet<StudentClass> StudentClasses { get; set; }

    public virtual DbSet<StudentGuardian> StudentGuardians { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC07E72A84E8");

            entity.ToTable("Attendance");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ClassId).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.EndDate)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.SessionType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Fixed");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.Class).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Attendance_Class_Id_fk");

            entity.HasOne(d => d.ClassSchedule).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ClassScheduleId)
                .HasConstraintName("FK_Attendance_ClassSchedule");
        });

        modelBuilder.Entity<AttendanceDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC079162DFE4");

            entity.ToTable("AttendanceDetail");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AttendanceId).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StudentClassId).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.Attendance).WithMany(p => p.AttendanceDetails)
                .HasForeignKey(d => d.AttendanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AttendanceDetail_Attendance_Id_fk");

            entity.HasOne(d => d.StudentClass).WithMany(p => p.AttendanceDetails)
                .HasForeignKey(d => d.StudentClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("AttendanceDetail_StudentClass_Id_fk");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Class__3214EC0762E79B13");

            entity.ToTable("Class");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.EndAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StartAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Classes)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Class__CreatedBy__46E78A0C");
        });

        modelBuilder.Entity<ClassSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ClassSch__3214EC077CA59604");

            entity.ToTable("ClassSchedule");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassSchedules)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClassSchedule_Class");
        });

        modelBuilder.Entity<Otp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OTP__3214EC078544DE14");

            entity.ToTable("OTP");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.ExpiredDate)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.IsUsed).HasColumnName("isUsed");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.UserId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.User).WithMany(p => p.Otps)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("OTP_User_Id_fk");
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Score__3214EC072819449D");

            entity.ToTable("Score");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ClassId).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.CreateBy)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");
            entity.Property(e => e.Shift)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.TestDateAt)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.Scores)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Score__ClassId__47DBAE45");

            entity.HasOne(d => d.CreateByNavigation).WithMany(p => p.Scores)
                .HasForeignKey(d => d.CreateBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Score__CreateBy__48CFD27E");
        });

        modelBuilder.Entity<ScoreDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ScoreDet__3214EC07D24DDF71");

            entity.ToTable("ScoreDetail");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Score)
                .HasDefaultValueSql("(NULL)")
                .HasColumnType("decimal(4, 2)");
            entity.Property(e => e.ScoreId).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StudentClassId).HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.ScoreNavigation).WithMany(p => p.ScoreDetails)
                .HasForeignKey(d => d.ScoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreDeta__Score__49C3F6B7");

            entity.HasOne(d => d.StudentClass).WithMany(p => p.ScoreDetails)
                .HasForeignKey(d => d.StudentClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ScoreDeta__Stude__4AB81AF0");
        });

        modelBuilder.Entity<StudentClass>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudentC__3214EC07119F651C");

            entity.ToTable("StudentClass");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ClassId).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StudentId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.Class).WithMany(p => p.StudentClasses)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCl__Class__4BAC3F29");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentClasses)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCl__Stude__4CA06362");
        });

        modelBuilder.Entity<StudentGuardian>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudentG__3214EC07FC1CC142");

            entity.HasIndex(e => new { e.GuardianId, e.StudentId }, "UQ_StudentGuardians_Student_Guardian").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.GuardianId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Relationship)
                .HasMaxLength(50)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StudentId)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");

            entity.HasOne(d => d.Guardian).WithMany(p => p.StudentGuardianGuardians)
                .HasForeignKey(d => d.GuardianId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentGuardians_User_2");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentGuardianStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentGuardians_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07FFE4B5D4");

            entity.ToTable("User");

            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)")
                .HasComment("");
            entity.Property(e => e.IsNeedResetPassword).HasComment("");
            entity.Property(e => e.Name)
                .HasMaxLength(300)
                .HasDefaultValueSql("(NULL)")
                .HasComment("");
            entity.Property(e => e.Password).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.PasswordBcrypt)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)")
                .HasColumnName("PasswordBCrypt");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)")
                .HasComment("");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)")
                .HasComment("");
            entity.Property(e => e.Salt).HasDefaultValueSql("(NULL)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)")
                .HasComment("");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
