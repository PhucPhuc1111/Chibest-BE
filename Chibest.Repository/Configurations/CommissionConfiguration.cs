using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class CommissionConfiguration : IEntityTypeConfiguration<Commission>
{
    public void Configure(EntityTypeBuilder<Commission> builder)
    {
                    builder.HasKey(e => e.Id).HasName("Commission_pkey");
        
                    builder.ToTable("Commission");
        
                    builder.HasIndex(e => new { e.EmployeeId, e.PeriodYear, e.PeriodMonth }, "ix_commission_employeeid").IsDescending(false, true, true);
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.Amount).HasColumnType("money");
                    builder.Property(e => e.CalculationBase).HasColumnType("money");
                    builder.Property(e => e.CommissionRate).HasPrecision(5, 2);
                    builder.Property(e => e.CommissionType).HasMaxLength(50);
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.ReferenceType).HasMaxLength(50);
        
                    builder.HasOne(d => d.Employee).WithMany(p => p.Commissions)
                        .HasForeignKey(d => d.EmployeeId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("Commission_EmployeeId_fkey");
    }
}
