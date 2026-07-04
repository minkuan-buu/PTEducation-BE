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

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<ChatDetail> ChatDetails { get; set; }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassSchedule> ClassSchedules { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<Otp> Otps { get; set; }

    public virtual DbSet<Score> Scores { get; set; }

    public virtual DbSet<ScoreDetail> ScoreDetails { get; set; }

    public virtual DbSet<StudentClass> StudentClasses { get; set; }

    public virtual DbSet<StudentGuardian> StudentGuardians { get; set; }

    public virtual DbSet<StudentTuition> StudentTuitions { get; set; }

    public virtual DbSet<TuitionPeriod> TuitionPeriods { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC07F481AFC8");

            entity.ToTable("Attendance");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Date).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.SessionType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Fixed");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Class).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Att_Class");

            entity.HasOne(d => d.ClassSchedule).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ClassScheduleId)
                .HasConstraintName("FK_Att_CS");
        });

        modelBuilder.Entity<AttendanceDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Attendan__3214EC070A4C753B");

            entity.ToTable("AttendanceDetail");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Attendance).WithMany(p => p.AttendanceDetailAttendances)
                .HasForeignKey(d => d.AttendanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AD_Att");

            entity.HasOne(d => d.MakeUpSessionNavigation).WithMany(p => p.AttendanceDetailMakeUpSessionNavigations)
                .HasForeignKey(d => d.MakeUpSession)
                .HasConstraintName("FK_AD_Att_01");

            entity.HasOne(d => d.StudentClass).WithMany(p => p.AttendanceDetails)
                .HasForeignKey(d => d.StudentClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AD_SC");
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Chat__3214EC07665A0511");

            entity.ToTable("Chat");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Class).WithMany(p => p.Chats)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_Chat_Class");
        });

        modelBuilder.Entity<ChatDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChatDeta__3214EC07BDD868E3");

            entity.ToTable("ChatDetail");

            entity.HasIndex(e => e.UserId, "IX_ChatDetail_UserId");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.UserId)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.Chat).WithMany(p => p.ChatDetails)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CD_Chat");

            entity.HasOne(d => d.LastReadMessage).WithMany(p => p.ChatDetails)
                .HasForeignKey(d => d.LastReadMessageId)
                .HasConstraintName("FK_CD_LastRead");

            entity.HasOne(d => d.User).WithMany(p => p.ChatDetails)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CD_User");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ChatMess__3214EC07E092441E");

            entity.ToTable("ChatMessage");

            entity.HasIndex(e => new { e.ChatId, e.CreatedAt }, "IX_ChatMessage_ChatId_CreatedAt");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

            entity.HasOne(d => d.Chat).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CM_Chat");

            entity.HasOne(d => d.SenderDetail).WithMany(p => p.ChatMessages)
                .HasForeignKey(d => d.SenderDetailId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CM_SenderDetail");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Class__3214EC078308503B");

            entity.ToTable("Class");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.EndAt).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasDefaultValueSql("(NULL)");
            entity.Property(e => e.StartAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Classes)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Class_User");

            entity.HasOne(d => d.Grade).WithMany(p => p.Classes)
                .HasForeignKey(d => d.GradeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Class_Grade");
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
                .HasConstraintName("FK_CS_Class");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Grade__3214EC07BC8F26CF");

            entity.ToTable("Grade");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.GradeName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Otp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OTP__3214EC07C63E98B9");

            entity.ToTable("OTP");

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
                .HasConstraintName("FK_OTP_User");
        });

        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Score__3214EC07ACF5C81E");

            entity.ToTable("Score");

            entity.Property(e => e.Id).ValueGeneratedNever();
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
                .HasConstraintName("FK_Score_Class");
        });

        modelBuilder.Entity<ScoreDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ScoreDet__3214EC07E01EB94E");

            entity.ToTable("ScoreDetail");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Note)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Score).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.ScoreNavigation).WithMany(p => p.ScoreDetails)
                .HasForeignKey(d => d.ScoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SD_Score");

            entity.HasOne(d => d.StudentClass).WithMany(p => p.ScoreDetails)
                .HasForeignKey(d => d.StudentClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SD_SC");
        });

        modelBuilder.Entity<StudentClass>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudentC__3214EC078425ABAC");

            entity.ToTable("StudentClass");

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
                .HasConstraintName("FK_SC_Class");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentClasses)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SC_User");
        });

        modelBuilder.Entity<StudentGuardian>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudentG__3214EC0713CAFB4B");

            entity.HasIndex(e => new { e.GuardianId, e.StudentId }, "UQ_StudentGuardians_Student_Guardian").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.GuardianId)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Relationship).HasMaxLength(50);
            entity.Property(e => e.StudentId)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.HasOne(d => d.Guardian).WithMany(p => p.StudentGuardianGuardians)
                .HasForeignKey(d => d.GuardianId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SG_Guardian");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentGuardianStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SG_Student");
        });

        modelBuilder.Entity<StudentTuition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StudentT__3214EC079B29F240");

            entity.ToTable("StudentTuition");

            entity.HasIndex(e => e.PaymentStatus, "IX_StudentTuition_Status");

            entity.HasIndex(e => new { e.TuitionPeriodId, e.StudentClassId }, "UQ_StudentTuition_Period_Student").IsUnique();

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.PaidAt).HasColumnType("datetime");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Unpaid");

            entity.HasOne(d => d.StudentClass).WithMany(p => p.StudentTuitions)
                .HasForeignKey(d => d.StudentClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ST_StudentClass");

            entity.HasOne(d => d.TuitionPeriod).WithMany(p => p.StudentTuitions)
                .HasForeignKey(d => d.TuitionPeriodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ST_Period");
        });

        modelBuilder.Entity<TuitionPeriod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TuitionP__3214EC078178108C");

            entity.ToTable("TuitionPeriod");

            entity.Property(e => e.Id).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TuitionPeriods)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Period_User");

            entity.HasOne(d => d.Grade).WithMany(p => p.TuitionPeriods)
                .HasForeignKey(d => d.GradeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Period_Grade");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC073E0D4F3B");

            entity.ToTable("User");

            entity.Property(e => e.Id)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(300);
            entity.Property(e => e.PasswordBcrypt)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("PasswordBCrypt");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SchoolInfo).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
