using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class PurchaseOrderDetailConfiguration : IEntityTypeConfiguration<PurchaseOrderDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderDetail> builder)
    {
                    builder.HasKey(e => e.Id).HasName("PurchaseOrderDetail_pkey");
        
                    builder.ToTable("PurchaseOrderDetail");
        
                    builder.HasIndex(e => e.PurchaseOrderId, "ix_transactionorderdetail_orderid");
        
                    builder.HasIndex(e => e.ProductId, "ix_transactionorderdetail_productid");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.ReFee).HasColumnType("money");
                    builder.Property(e => e.UnitPrice).HasColumnType("money");
        
                    builder.HasOne(d => d.Product).WithMany(p => p.PurchaseOrderDetails)
                        .HasForeignKey(d => d.ProductId)
                        .HasConstraintName("PurchaseOrderDetail_ProductId_fkey");
        
                    builder.HasOne(d => d.PurchaseOrder).WithMany(p => p.PurchaseOrderDetails)
                        .HasForeignKey(d => d.PurchaseOrderId)
                        .HasConstraintName("PurchaseOrderDetail_PurchaseOrderId_fkey");
    }
}
