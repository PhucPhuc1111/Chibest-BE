using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class PayrollConfiguration : IEntityTypeConfiguration<Payroll>
{
    public void Configure(EntityTypeBuilder<Payroll> builder)
    {
                    builder.HasKey(e => e.Id).HasName("Payroll_pkey");
        
                    builder.ToTable("Payroll");
        
                    builder.HasIndex(e => new { e.EmployeeId, e.PeriodYear, e.PeriodMonth }, "ix_payroll_employeeid").IsDescending(false, true, true);
        
                    builder.HasIndex(e => new { e.PaymentStatus, e.PeriodYear, e.PeriodMonth }, "ix_payroll_status").IsDescending(false, true, true);
        
                    builder.HasIndex(e => new { e.EmployeeId, e.PeriodYear, e.PeriodMonth }, "uq_payroll_employee_period").IsUnique();
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.ActualBaseSalary).HasColumnType("money");
                    builder.Property(e => e.BaseSalary).HasColumnType("money");
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.GrossSalary)
                        .HasComputedColumnSql("((((\"ActualBaseSalary\" + \"TotalAllowance\") + \"OvertimeSalary\") + \"TotalCommission\") + \"TotalBonus\")", true)
                        .HasColumnType("money");
                    builder.Property(e => e.HealthInsurance).HasColumnType("money");
                    builder.Property(e => e.NetSalary)
                        .HasComputedColumnSql("((((((((\"ActualBaseSalary\" + \"TotalAllowance\") + \"OvertimeSalary\") + \"TotalCommission\") + \"TotalBonus\") - \"TotalDeduction\") - \"SocialInsurance\") - \"HealthInsurance\") - \"UnemploymentInsurance\")", true)
                        .HasColumnType("money");
                    builder.Property(e => e.OvertimeSalary).HasColumnType("money");
                    builder.Property(e => e.PaymentMethod)
                        .HasMaxLength(50)
                        .HasDefaultValueSql("'Chuyển Khoản'::character varying");
                    builder.Property(e => e.PaymentStatus)
                        .HasMaxLength(50)
                        .HasDefaultValueSql("'Chờ Thanh Toán'::character varying");
                    builder.Property(e => e.SocialInsurance).HasColumnType("money");
                    builder.Property(e => e.StandardWorkDays).HasDefaultValue(26);
                    builder.Property(e => e.TotalAllowance).HasColumnType("money");
                    builder.Property(e => e.TotalBonus).HasColumnType("money");
                    builder.Property(e => e.TotalCommission).HasColumnType("money");
                    builder.Property(e => e.TotalDeduction).HasColumnType("money");
                    builder.Property(e => e.TotalOvertimeHours).HasPrecision(10, 2);
                    builder.Property(e => e.TotalWorkDays).HasDefaultValue(0);
                    builder.Property(e => e.TotalWorkHours).HasPrecision(10, 2);
                    builder.Property(e => e.UnemploymentInsurance).HasColumnType("money");
                    builder.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
        
                    builder.HasOne(d => d.Branch).WithMany(p => p.Payrolls)
                        .HasForeignKey(d => d.BranchId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("Payroll_BranchId_fkey");
        
                    builder.HasOne(d => d.Employee).WithMany(p => p.Payrolls)
                        .HasForeignKey(d => d.EmployeeId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("Payroll_EmployeeId_fkey");
    }
}
