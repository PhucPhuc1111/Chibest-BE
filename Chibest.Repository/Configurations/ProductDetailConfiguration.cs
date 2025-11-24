using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class ProductDetailConfiguration : IEntityTypeConfiguration<ProductDetail>
{
    public void Configure(EntityTypeBuilder<ProductDetail> builder)
    {
                    builder.HasKey(e => e.Id).HasName("ProductDetail_pkey");
        
                    builder.ToTable("ProductDetail");
        
                    builder.HasIndex(e => e.ChipCode, "ProductDetail_ChipCode_key").IsUnique();
        
                    builder.HasIndex(e => e.TagId, "ProductDetail_TagId_key").IsUnique();
        
                    builder.HasIndex(e => e.ChipCode, "ix_productdetail_chipcode");
        
                    builder.HasIndex(e => e.ProductId, "ix_productdetail_productid");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.ChipCode).HasMaxLength(100);
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.TagId).HasMaxLength(100);
                    builder.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
        
                    builder.HasOne(d => d.Product).WithMany(p => p.ProductDetails)
                        .HasForeignKey(d => d.ProductId)
                        .HasConstraintName("ProductDetail_ProductId_fkey");
    }
}
