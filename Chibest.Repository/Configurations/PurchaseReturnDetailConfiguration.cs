using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class PurchaseReturnDetailConfiguration : IEntityTypeConfiguration<PurchaseReturnDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnDetail> builder)
    {
                    builder.HasKey(e => e.Id).HasName("PurchaseReturnDetail_pkey");
        
                    builder.ToTable("PurchaseReturnDetail");
        
                    builder.HasIndex(e => e.PurchaseReturnId, "ix_purchasereturndetail_orderid");
        
                    builder.HasIndex(e => e.ProductId, "ix_purchasereturndetail_productid");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.UnitPrice).HasColumnType("money");
        
                    builder.HasOne(d => d.Product).WithMany(p => p.PurchaseReturnDetails)
                        .HasForeignKey(d => d.ProductId)
                        .HasConstraintName("PurchaseReturnDetail_ProductId_fkey");
        
                    builder.HasOne(d => d.PurchaseReturn).WithMany(p => p.PurchaseReturnDetails)
                        .HasForeignKey(d => d.PurchaseReturnId)
                        .HasConstraintName("PurchaseReturnDetail_PurchaseReturnId_fkey");
    }
}
