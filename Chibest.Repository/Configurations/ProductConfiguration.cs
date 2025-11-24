using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
                    builder.HasKey(e => e.Id).HasName("Product_pkey");
        
                    builder.ToTable("Product");
        
                    builder.HasIndex(e => e.BarCode, "Product_BarCode_key").IsUnique();
        
                    builder.HasIndex(e => e.Sku, "Product_SKU_key").IsUnique();
        
                    builder.HasIndex(e => e.CategoryId, "ix_product_categoryid");
        
                    builder.HasIndex(e => e.ColorId, "ix_product_colorid");
        
                    builder.HasIndex(e => e.Name, "ix_product_name");
        
                    builder.HasIndex(e => e.ParentSku, "ix_product_parentsku");
        
                    builder.HasIndex(e => e.SizeId, "ix_product_sizeid");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.AvatarUrl).HasColumnName("AvatarURL");
                    builder.Property(e => e.BarCode).HasMaxLength(100);
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.IsMaster).HasDefaultValue(true);
                    builder.Property(e => e.Material).HasMaxLength(100);
                    builder.Property(e => e.Name).HasMaxLength(250);
                    builder.Property(e => e.Note).HasMaxLength(200);
                    builder.Property(e => e.ParentSku)
                        .HasMaxLength(50)
                        .HasColumnName("ParentSKU");
                    builder.Property(e => e.Sku)
                        .HasMaxLength(50)
                        .HasColumnName("SKU");
                    builder.Property(e => e.Status)
                        .HasMaxLength(40)
                        .HasDefaultValueSql("'UnAvailable'::character varying");
                    builder.Property(e => e.Style).HasMaxLength(100);
                    builder.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.VideoUrl).HasColumnName("VideoURL");
                    builder.Property(e => e.Weight).HasDefaultValue(0);
        
                    builder.HasOne(d => d.Category).WithMany(p => p.Products)
                        .HasForeignKey(d => d.CategoryId)
                        .HasConstraintName("Product_CategoryId_fkey");
        
                    builder.HasOne(d => d.Color).WithMany(p => p.Products)
                        .HasForeignKey(d => d.ColorId)
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("Product_ColorId_fkey");
        
                    builder.HasOne(d => d.ParentSkuNavigation).WithMany(p => p.InverseParentSkuNavigation)
                        .HasPrincipalKey(p => p.Sku)
                        .HasForeignKey(d => d.ParentSku)
                        .HasConstraintName("Product_ParentSKU_fkey");
        
                    builder.HasOne(d => d.Size).WithMany(p => p.Products)
                        .HasForeignKey(d => d.SizeId)
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("Product_SizeId_fkey");
    }
}
