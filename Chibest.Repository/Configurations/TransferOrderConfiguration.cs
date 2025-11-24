using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class TransferOrderConfiguration : IEntityTypeConfiguration<TransferOrder>
{
    public void Configure(EntityTypeBuilder<TransferOrder> builder)
    {
                    builder.HasKey(e => e.Id).HasName("TransferOrder_pkey");
        
                    builder.ToTable("TransferOrder");
        
                    builder.HasIndex(e => e.InvoiceCode, "TransferOrder_InvoiceCode_key").IsUnique();
        
                    builder.HasIndex(e => new { e.FromBranch, e.ToBranch, e.OrderDate }, "ix_transferorder_branch").IsDescending(false, false, true);
        
                    builder.HasIndex(e => e.OrderDate, "ix_transferorder_orderdate").IsDescending();
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.InvoiceCode).HasMaxLength(100);
                    builder.Property(e => e.OrderDate)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.Status)
                        .HasMaxLength(40)
                        .HasDefaultValueSql("'Draft'::character varying");
                    builder.Property(e => e.SubTotal).HasColumnType("money");
                    builder.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
        
                    builder.HasOne(d => d.Employee).WithMany(p => p.TransferOrders)
                        .HasForeignKey(d => d.EmployeeId)
                        .HasConstraintName("TransferOrder_EmployeeId_fkey");
        
                    builder.HasOne(d => d.FromBranchNavigation).WithMany(p => p.TransferOrderFromBranchNavigations)
                        .HasForeignKey(d => d.FromBranch)
                        .HasConstraintName("TransferOrder_FromBranch_fkey");
        
                    builder.HasOne(d => d.ToBranchNavigation).WithMany(p => p.TransferOrderToBranchNavigations)
                        .HasForeignKey(d => d.ToBranch)
                        .HasConstraintName("TransferOrder_ToBranch_fkey");
    }
}
