using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class FranchiseOrderConfiguration : IEntityTypeConfiguration<FranchiseOrder>
{
    public void Configure(EntityTypeBuilder<FranchiseOrder> builder)
    {
                    builder.HasKey(e => e.Id).HasName("FranchiseOrder_pkey");
        
                    builder.ToTable("FranchiseOrder");
        
                    builder.HasIndex(e => e.InvoiceCode, "FranchiseOrder_InvoiceCode_key").IsUnique();
        
                    builder.HasIndex(e => e.BranchId, "ix_franchiseorder_branchid");
        
                    builder.HasIndex(e => e.FranchiseInvoiceId, "ix_franchiseorder_invoiceid");
        
                    builder.HasIndex(e => e.OrderDate, "ix_franchiseorder_orderdate").IsDescending();
        
                    builder.HasIndex(e => e.Status, "ix_franchiseorder_status");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.InvoiceCode).HasMaxLength(100);
                    builder.Property(e => e.OrderDate)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.Status)
                        .HasMaxLength(40)
                        .HasDefaultValueSql("'Draft'::character varying");
                    builder.Property(e => e.TotalMoney).HasColumnType("money");
        
                    builder.HasOne(d => d.Branch).WithMany(p => p.FranchiseOrders)
                        .HasForeignKey(d => d.BranchId)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FranchiseOrder_BranchId_fkey");
        
                    builder.HasOne(d => d.FranchiseInvoice).WithMany(p => p.FranchiseOrders)
                        .HasForeignKey(d => d.FranchiseInvoiceId)
                        .HasConstraintName("FranchiseOrder_FranchiseInvoiceId_fkey");
    }
}

