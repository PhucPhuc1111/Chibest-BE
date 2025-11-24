using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
                    builder.HasKey(e => e.Id).HasName("StockAdjustment_pkey");
        
                    builder.ToTable("StockAdjustment");
        
                    builder.HasIndex(e => e.AdjustmentCode, "StockAdjustment_AdjustmentCode_key").IsUnique();
        
                    builder.HasIndex(e => new { e.BranchId, e.AdjustmentDate }, "ix_stockadjustment_branchid").IsDescending(false, true);
        
                    builder.HasIndex(e => new { e.AdjustmentType, e.AdjustmentDate }, "ix_stockadjustment_type_date").IsDescending(false, true);
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.AdjustmentCode).HasMaxLength(100);
                    builder.Property(e => e.AdjustmentDate)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.AdjustmentType).HasMaxLength(50);
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.Status)
                        .HasMaxLength(40)
                        .HasDefaultValueSql("'Draft'::character varying");
                    builder.Property(e => e.TotalValueChange).HasColumnType("money");
                    builder.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
        
                    builder.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.StockAdjustmentApprovedByNavigations)
                        .HasForeignKey(d => d.ApprovedBy)
                        .HasConstraintName("StockAdjustment_ApprovedBy_fkey");
        
                    builder.HasOne(d => d.Branch).WithMany(p => p.StockAdjustments)
                        .HasForeignKey(d => d.BranchId)
                        .HasConstraintName("StockAdjustment_BranchId_fkey");
        
                    builder.HasOne(d => d.Employee).WithMany(p => p.StockAdjustmentEmployees)
                        .HasForeignKey(d => d.EmployeeId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("StockAdjustment_EmployeeId_fkey");
    }
}
