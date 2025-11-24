using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class SupplierDebtHistoryConfiguration : IEntityTypeConfiguration<SupplierDebtHistory>
{
    public void Configure(EntityTypeBuilder<SupplierDebtHistory> builder)
    {
                    builder.HasKey(e => e.Id).HasName("SupplierDebtHistory_pkey");
        
                    builder.ToTable("SupplierDebtHistory");
        
                    builder.HasIndex(e => new { e.SupplierDebtId, e.TransactionDate }, "ix_supplierdebthistory_supplier").IsDescending(false, true);
        
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
        
                    builder.HasOne(d => d.SupplierDebt).WithMany(p => p.SupplierDebtHistories)
                        .HasForeignKey(d => d.SupplierDebtId)
                        .HasConstraintName("SupplierDebtHistory_SupplierDebtId_fkey");
    }
}
