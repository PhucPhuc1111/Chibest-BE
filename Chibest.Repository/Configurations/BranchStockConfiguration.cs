using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class BranchStockConfiguration : IEntityTypeConfiguration<BranchStock>
{
    public void Configure(EntityTypeBuilder<BranchStock> builder)
    {
                    builder.HasKey(e => e.Id).HasName("BranchStock_pkey");
        
                    builder.ToTable("BranchStock");
        
                    builder.HasIndex(e => new { e.BranchId, e.AvailableQty }, "ix_branchstock_branchid");
        
                    builder.HasIndex(e => e.ProductId, "ix_branchstock_productid");
        
                    builder.HasIndex(e => new { e.ProductId, e.BranchId }, "uq_branchstock_product_branch").IsUnique();
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.AvailableQty).HasDefaultValue(0);
                    builder.Property(e => e.MaximumStock).HasDefaultValue(0);
                    builder.Property(e => e.MinimumStock).HasDefaultValue(0);
        
                    builder.HasOne(d => d.Branch).WithMany(p => p.BranchStocks)
                        .HasForeignKey(d => d.BranchId)
                        .HasConstraintName("BranchStock_BranchId_fkey");
        
                    builder.HasOne(d => d.Product).WithMany(p => p.BranchStocks)
                        .HasForeignKey(d => d.ProductId)
                        .HasConstraintName("BranchStock_ProductId_fkey");
    }
}
