using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class PurchaseInvoiceConfiguration : IEntityTypeConfiguration<PurchaseInvoice>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoice> builder)
    {
                    builder.HasKey(e => e.Id).HasName("PurchaseInvoice_pkey");
        
                    builder.ToTable("PurchaseInvoice");
        
                    builder.HasIndex(e => e.Code, "PurchaseInvoice_Code_key").IsUnique();
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.Code).HasMaxLength(100);
                    builder.Property(e => e.OrderDate)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.Status)
                        .HasMaxLength(40)
                        .HasDefaultValueSql("'Draft'::character varying");
                    builder.Property(e => e.TotalMoney).HasColumnType("money");
        
                    builder.HasOne(d => d.Supplier).WithMany(p => p.PurchaseInvoices)
                        .HasForeignKey(d => d.SupplierId)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("PurchaseInvoice_SupplierId_fkey");
    }
}

