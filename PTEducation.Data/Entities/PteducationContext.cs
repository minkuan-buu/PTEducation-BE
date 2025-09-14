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

    public PteducationContext()
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<AttendanceDetail> AttendanceDetails { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    public virtual DbSet<Score> Scores { get; set; }

    public virtual DbSet<ScoreDetail> ScoreDetails { get; set; }

    public virtual DbSet<StudentClass> StudentClasses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Attendance_pk");

            entity.ToTable("Attendance");

            entity.HasIndex(e => e.ClassId, "IX_Attendance_ClassId");

            entity.HasIndex(e => e.CreatedBy, "IX_Attendance_CreatedBy");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Class).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Attendance_Class_Id_fk");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Attendance_User_Id_fk");
        });

        modelBuilder.Entity<AttendanceDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("AttendanceDetail_pk");

            entity.ToTable("AttendanceDetail");

            entity.HasIndex(e => e.AttendanceId, "IX_AttendanceDetail_AttendanceId");

            entity.HasIndex(e => e.StudentClassId, "IX_AttendanceDetail_StudentClassId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

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
            entity.HasKey(e => e.Id).HasName("PK__Class__3214EC075D19815A");

            entity.ToTable("Class");

            entity.HasIndex(e => e.CreatedBy, "IX_Class_CreatedBy");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.EndAt).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.StartAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Classes)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Class__CreatedBy__46E78A0C");
        });

        modelBuilder.Entity<Otp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("OTP_pk");

            entity.ToTable("OTP");

            entity.HasIndex(e => e.UserId, "IX_OTP_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.ExpiredDate).HasColumnType("datetime");
            entity.Property(e => e.IsUsed).HasColumnName("isUsed");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UserId)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Otps)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("OTP_User_Id_fk");
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Score__3214EC07064E064E");

            entity.ToTable("Score");

            entity.HasIndex(e => e.ClassId, "IX_Score_ClassId");

            entity.HasIndex(e => e.CreateBy, "IX_Score_CreateBy");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreateBy)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.Shift)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TestDateAt).HasColumnType("datetime");

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
            entity.HasKey(e => e.Id).HasName("PK__ScoreDet__3214EC076B71B76A");

            entity.ToTable("ScoreDetail");

            entity.HasIndex(e => e.ScoreId, "IX_ScoreDetail_ScoreId");

            entity.HasIndex(e => e.StudentClassId, "IX_ScoreDetail_StudentClassId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Score).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

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
            entity.HasKey(e => e.Id).HasName("PK__StudentC__3214EC076DB43B81");

            entity.ToTable("StudentClass");

            entity.HasIndex(e => e.ClassId, "IX_StudentClass_ClassId");

            entity.HasIndex(e => e.StudentId, "IX_StudentClass_StudentId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StudentId)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.Class).WithMany(p => p.StudentClasses)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCl__Class__4BAC3F29");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentClasses)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentCl__Stude__4CA06362");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC075BB62C12");

            entity.ToTable("User");

            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
