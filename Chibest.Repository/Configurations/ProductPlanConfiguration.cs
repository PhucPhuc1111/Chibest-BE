using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class ProductPlanConfiguration : IEntityTypeConfiguration<ProductPlan>
{
    public void Configure(EntityTypeBuilder<ProductPlan> builder)
    {
                    builder.HasKey(e => e.Id).HasName("ProductPlan_pkey");
        
                    builder.ToTable("ProductPlan");
        
                    builder.HasIndex(e => e.ProductId, "IX_ProductPlan_ProductId");
        
                    builder.HasIndex(e => e.SupplierId, "IX_ProductPlan_SupplierId");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.DetailAmount).HasMaxLength(100);
                    builder.Property(e => e.Note).HasMaxLength(100);
                    builder.Property(e => e.SendDate)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.Status)
                        .HasMaxLength(40)
                        .HasDefaultValueSql("'Queue'::character varying");
                    builder.Property(e => e.Type).HasMaxLength(100);
                    builder.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
        
                    builder.HasOne(d => d.Product).WithMany(p => p.ProductPlans)
                        .HasForeignKey(d => d.ProductId)
                        .HasConstraintName("ProductPlan_ProductId_fkey");
        
                    builder.HasOne(d => d.Supplier).WithMany(p => p.ProductPlans)
                        .HasForeignKey(d => d.SupplierId)
                        .HasConstraintName("ProductPlan_SupplierId_fkey");
    }
}
