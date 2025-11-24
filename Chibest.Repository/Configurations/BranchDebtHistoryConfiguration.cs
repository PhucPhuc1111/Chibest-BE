using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class BranchDebtHistoryConfiguration : IEntityTypeConfiguration<BranchDebtHistory>
{
    public void Configure(EntityTypeBuilder<BranchDebtHistory> builder)
    {
                    builder.HasKey(e => e.Id).HasName("BranchDebtHistory_pkey");
        
                    builder.ToTable("BranchDebtHistory");
        
                    builder.HasIndex(e => new { e.BranchDebtId, e.TransactionDate }, "IX_BranchDebtHistory_Branch").IsDescending(false, true);
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.Amount).HasColumnType("money");
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.Status)
                        .HasMaxLength(40)
                        .HasDefaultValueSql("'Pending'::character varying");
                    builder.Property(e => e.TransactionDate)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.TransactionType).HasMaxLength(50);
        
                    builder.HasOne(d => d.BranchDebt).WithMany(p => p.BranchDebtHistories)
                        .HasForeignKey(d => d.BranchDebtId)
                        .HasConstraintName("BranchDebtHistory_BranchDebtId_fkey");
    }
}
