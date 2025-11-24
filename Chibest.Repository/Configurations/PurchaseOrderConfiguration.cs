using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
                    builder.HasKey(e => e.Id).HasName("PurchaseOrder_pkey");
        
                    builder.ToTable("PurchaseOrder");
        
                    builder.HasIndex(e => e.InvoiceCode, "PurchaseOrder_InvoiceCode_key").IsUnique();
        
                    builder.HasIndex(e => e.OrderDate, "ix_transactionorder_orderdate").IsDescending();
        
                    builder.HasIndex(e => e.Status, "ix_transactionorder_status");
        
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
        
                    builder.HasOne(d => d.Branch).WithMany(p => p.PurchaseOrders)
                        .HasForeignKey(d => d.BranchId)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("PurchaseOrder_BranchId_fkey");
        
                    builder.HasOne(d => d.Employee).WithMany(p => p.PurchaseOrderEmployees)
                        .HasForeignKey(d => d.EmployeeId)
                        .HasConstraintName("PurchaseOrder_EmployeeId_fkey");
        
                    builder.HasOne(d => d.Supplier).WithMany(p => p.PurchaseOrderSuppliers)
                        .HasForeignKey(d => d.SupplierId)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("PurchaseOrder_SupplierId_fkey");
    }
}
