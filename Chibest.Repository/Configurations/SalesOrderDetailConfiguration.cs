using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class SalesOrderDetailConfiguration : IEntityTypeConfiguration<SalesOrderDetail>
{
    public void Configure(EntityTypeBuilder<SalesOrderDetail> builder)
    {
                    builder.HasKey(e => e.Id).HasName("SalesOrderDetail_pkey");
        
                    builder.ToTable("SalesOrderDetail");
        
                    builder.HasIndex(e => e.SalesOrderId, "ix_salesorderdetail_orderid");
        
                    builder.HasIndex(e => e.ProductId, "ix_salesorderdetail_productid");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.DiscountAmount).HasColumnType("money");
                    builder.Property(e => e.DiscountPercent).HasPrecision(5, 2);
                    builder.Property(e => e.ItemName).HasMaxLength(250);
                    builder.Property(e => e.ProductSku)
                        .HasMaxLength(50)
                        .HasColumnName("ProductSKU");
                    builder.Property(e => e.TotalPrice)
                        .HasComputedColumnSql("(((\"Quantity\" * \"UnitPrice\") - \"DiscountAmount\") + ((\"Quantity\" * \"UnitPrice\") / 100))", true)
                        .HasColumnType("money");
                    builder.Property(e => e.UnitPrice).HasColumnType("money");
        
                    builder.HasOne(d => d.ProductDetail).WithMany(p => p.SalesOrderDetails)
                        .HasForeignKey(d => d.ProductDetailId)
                        .HasConstraintName("SalesOrderDetail_ProductDetailId_fkey");
        
                    builder.HasOne(d => d.Product).WithMany(p => p.SalesOrderDetails)
                        .HasForeignKey(d => d.ProductId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("SalesOrderDetail_ProductId_fkey");
        
                    builder.HasOne(d => d.SalesOrder).WithMany(p => p.SalesOrderDetails)
                        .HasForeignKey(d => d.SalesOrderId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("SalesOrderDetail_SalesOrderId_fkey");
    }
}
