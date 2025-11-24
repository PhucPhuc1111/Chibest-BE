using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
                    builder.HasKey(e => e.Id).HasName("Attendance_pkey");
        
                    builder.ToTable("Attendance");
        
                    builder.HasIndex(e => new { e.BranchId, e.WorkDate }, "ix_attendance_branchid").IsDescending(false, true);
        
                    builder.HasIndex(e => new { e.EmployeeId, e.WorkDate }, "ix_attendance_employeeid").IsDescending(false, true);
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.AttendanceStatus)
                        .HasMaxLength(50)
                        .HasDefaultValueSql("'Có Mặt'::character varying");
                    builder.Property(e => e.CheckInTime).HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.CheckOutTime).HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.DayType)
                        .HasMaxLength(50)
                        .HasDefaultValueSql("'Ngày Thường'::character varying");
                    builder.Property(e => e.OvertimeHours).HasPrecision(5, 2);
                    builder.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.WorkHours).HasPrecision(5, 2);
        
                    builder.HasOne(d => d.Branch).WithMany(p => p.Attendances)
                        .HasForeignKey(d => d.BranchId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("Attendance_BranchId_fkey");
        
                    builder.HasOne(d => d.Employee).WithMany(p => p.Attendances)
                        .HasForeignKey(d => d.EmployeeId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("Attendance_EmployeeId_fkey");
        
                    builder.HasOne(d => d.WorkShift).WithMany(p => p.Attendances)
                        .HasForeignKey(d => d.WorkShiftId)
                        .HasConstraintName("Attendance_WorkShiftId_fkey");
    }
}
