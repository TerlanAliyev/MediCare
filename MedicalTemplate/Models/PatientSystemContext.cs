using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MedicalTemplate.Models;

public partial class PatientSystemContext : DbContext
{
    public PatientSystemContext()
    {
    }

    public PatientSystemContext(DbContextOptions<PatientSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<DaysOfWeek> DaysOfWeeks { get; set; }

    public virtual DbSet<DoctorDatum> DoctorData { get; set; }

    public virtual DbSet<DoctorTimeSchedule> DoctorTimeSchedules { get; set; }

    public virtual DbSet<OperationType> OperationTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-353APLF\\SQLEXPRESS;Database=PatientSystem;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__Appointm__8ECDFCC294EB2B3B");

            entity.Property(e => e.AppointmentEndTime).HasColumnType("datetime");
            entity.Property(e => e.AppointmentStartTime).HasColumnType("datetime");

            entity.HasOne(d => d.AppointmentDr).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.AppointmentDrId)
                .HasConstraintName("FK_Appointments_AppointmentDrId");

            entity.HasOne(d => d.AppointmentUser).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.AppointmentUserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Appointments_AppointmentUserId");

            entity.HasOne(d => d.OperationType).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.OperationTypeId)
                .HasConstraintName("FK_Appointments_OperationTypeId");
        });

        modelBuilder.Entity<DaysOfWeek>(entity =>
        {
            entity.HasKey(e => e.DaysId).HasName("PK__DaysOfWe__BCE78B2BC35E54AA");

            entity.ToTable("DaysOfWeek");
        });

        modelBuilder.Entity<DoctorDatum>(entity =>
        {
            entity.HasKey(e => e.DrId).HasName("PK__DoctorDa__0150EEFB64F8B578");

            entity.Property(e => e.DrAdress).HasMaxLength(100);
            entity.Property(e => e.DrCity).HasMaxLength(100);
            entity.Property(e => e.DrContext).HasMaxLength(1000);
            entity.Property(e => e.DrEmail).HasMaxLength(100);
            entity.Property(e => e.DrLastName).HasMaxLength(100);
            entity.Property(e => e.DrName).HasMaxLength(100);
            entity.Property(e => e.DrPassword).HasMaxLength(100);
            entity.Property(e => e.DrPhoneNumber).HasMaxLength(100);
            entity.Property(e => e.DrSubTitle).HasMaxLength(1000);
            entity.Property(e => e.DrTitle).HasMaxLength(100);
        });

        modelBuilder.Entity<DoctorTimeSchedule>(entity =>
        {
            entity.HasKey(e => e.DrTimeId).HasName("PK__DoctorTi__2E898D120455372D");

            entity.ToTable("DoctorTimeSchedule");

            entity.Property(e => e.BreakEnd).HasColumnType("datetime");
            entity.Property(e => e.BreakStart).HasColumnType("datetime");
            entity.Property(e => e.DrEndTime).HasColumnType("datetime");
            entity.Property(e => e.DrStartTime).HasColumnType("datetime");

            entity.HasOne(d => d.DrDay).WithMany(p => p.DoctorTimeSchedules)
                .HasForeignKey(d => d.DrDayId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__DoctorTim__DrDay__4D94879B");

            entity.HasOne(d => d.Dr).WithMany(p => p.DoctorTimeSchedules)
                .HasForeignKey(d => d.DrId)
                .HasConstraintName("FK__DoctorTime__DrId__398D8EEE");
        });

        modelBuilder.Entity<OperationType>(entity =>
        {
            entity.HasKey(e => e.OpId).HasName("PK__Operatio__46E40E08325BD8B5");

            entity.Property(e => e.OpName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C0617E633");

            entity.Property(e => e.UserComplate)
                .HasMaxLength(10)
                .HasDefaultValue("UnComplated");
            entity.Property(e => e.UserEmail).HasMaxLength(30);
            entity.Property(e => e.UserFirstName).HasMaxLength(20);
            entity.Property(e => e.UserLastName).HasMaxLength(20);
            entity.Property(e => e.UserMessage).HasMaxLength(50);
            entity.Property(e => e.UserOption)
                .HasMaxLength(10)
                .HasDefaultValue("No");
            entity.Property(e => e.UserPhoneNumber).HasMaxLength(50);
            entity.Property(e => e.UserPostDate).HasColumnType("datetime");

            entity.HasOne(d => d.UserChoosenDr).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserChoosenDrId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Users__UserChoos__4AB81AF0");

            entity.HasOne(d => d.UserOperationType).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserOperationTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Users__UserOpera__52593CB8");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
