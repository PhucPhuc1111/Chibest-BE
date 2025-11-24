using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class DeductionConfiguration : IEntityTypeConfiguration<Deduction>
{
    public void Configure(EntityTypeBuilder<Deduction> builder)
    {
                    builder.HasKey(e => e.Id).HasName("Deduction_pkey");
        
                    builder.ToTable("Deduction");
        
                    builder.HasIndex(e => new { e.EmployeeId, e.PeriodYear, e.PeriodMonth }, "ix_deduction_employeeid").IsDescending(false, true, true);
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.Amount).HasColumnType("money");
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.DeductionType).HasMaxLength(50);
        
                    builder.HasOne(d => d.Employee).WithMany(p => p.Deductions)
                        .HasForeignKey(d => d.EmployeeId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("Deduction_EmployeeId_fkey");
    }
}
