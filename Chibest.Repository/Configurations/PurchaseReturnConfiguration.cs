using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
    {
                    builder.HasKey(e => e.Id).HasName("PurchaseReturn_pkey");
        
                    builder.ToTable("PurchaseReturn");
        
                    builder.HasIndex(e => e.InvoiceCode, "PurchaseReturn_InvoiceCode_key").IsUnique();
        
                    builder.HasIndex(e => e.BranchId, "ix_purchasereturn_branch");
        
                    builder.HasIndex(e => e.OrderDate, "ix_purchasereturn_orderdate").IsDescending();
        
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
        
                    builder.HasOne(d => d.Branch).WithMany(p => p.PurchaseReturns)
                        .HasForeignKey(d => d.BranchId)
                        .HasConstraintName("fk_purchasereturn_branch");
        
                    builder.HasOne(d => d.Employee).WithMany(p => p.PurchaseReturnEmployees)
                        .HasForeignKey(d => d.EmployeeId)
                        .HasConstraintName("fk_purchasereturn_employee");
        
                    builder.HasOne(d => d.Supplier).WithMany(p => p.PurchaseReturnSuppliers)
                        .HasForeignKey(d => d.SupplierId)
                        .HasConstraintName("fk_purchasereturn_supplier");
    }
}
