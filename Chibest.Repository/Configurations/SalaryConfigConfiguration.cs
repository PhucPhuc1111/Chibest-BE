using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class SalaryConfigConfiguration : IEntityTypeConfiguration<SalaryConfig>
{
    public void Configure(EntityTypeBuilder<SalaryConfig> builder)
    {
                    builder.HasKey(e => e.Id).HasName("SalaryConfig_pkey");
        
                    builder.ToTable("SalaryConfig");
        
                    builder.HasIndex(e => new { e.EmployeeId, e.EffectiveDate }, "ix_salaryconfig_employeeid").IsDescending(false, true);
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.BaseSalary).HasColumnType("money");
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.HolidayCoefficient)
                        .HasPrecision(5, 2)
                        .HasDefaultValueSql("2.0");
                    builder.Property(e => e.HourlyRate).HasColumnType("money");
                    builder.Property(e => e.HousingAllowance).HasColumnType("money");
                    builder.Property(e => e.MealAllowance).HasColumnType("money");
                    builder.Property(e => e.OvertimeCoefficient)
                        .HasPrecision(5, 2)
                        .HasDefaultValueSql("1.5");
                    builder.Property(e => e.PhoneAllowance).HasColumnType("money");
                    builder.Property(e => e.PositionAllowance).HasColumnType("money");
                    builder.Property(e => e.SalaryType)
                        .HasMaxLength(50)
                        .HasDefaultValueSql("'Theo Tháng'::character varying");
                    builder.Property(e => e.Status)
                        .HasMaxLength(40)
                        .HasDefaultValueSql("'Đang Áp Dụng'::character varying");
                    builder.Property(e => e.TransportAllowance).HasColumnType("money");
                    builder.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.WeekendCoefficient)
                        .HasPrecision(5, 2)
                        .HasDefaultValueSql("1.3");
        
                    builder.HasOne(d => d.Branch).WithMany(p => p.SalaryConfigs)
                        .HasForeignKey(d => d.BranchId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("SalaryConfig_BranchId_fkey");
        
                    builder.HasOne(d => d.Employee).WithMany(p => p.SalaryConfigs)
                        .HasForeignKey(d => d.EmployeeId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("SalaryConfig_EmployeeId_fkey");
    }
}
