using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class ProductPriceHistoryConfiguration : IEntityTypeConfiguration<ProductPriceHistory>
{
    public void Configure(EntityTypeBuilder<ProductPriceHistory> builder)
    {
                    builder.HasKey(e => e.Id).HasName("ProductPriceHistory_pkey");
        
                    builder.ToTable("ProductPriceHistory");
        
                    builder.HasIndex(e => new { e.ProductId, e.BranchId, e.EffectiveDate }, "ix_productpricehistory_product_branch").IsDescending(false, false, true);
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.CostPrice).HasColumnType("money");
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.EffectiveDate)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.ExpiryDate).HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.SellingPrice).HasColumnType("money");
        
                    builder.HasOne(d => d.Branch).WithMany(p => p.ProductPriceHistories)
                        .HasForeignKey(d => d.BranchId)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("ProductPriceHistory_BranchId_fkey");
        
                    builder.HasOne(d => d.Product).WithMany(p => p.ProductPriceHistories)
                        .HasForeignKey(d => d.ProductId)
                        .HasConstraintName("ProductPriceHistory_ProductId_fkey");
    }
}
