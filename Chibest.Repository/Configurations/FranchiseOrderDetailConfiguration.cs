using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class FranchiseOrderDetailConfiguration : IEntityTypeConfiguration<FranchiseOrderDetail>
{
    public void Configure(EntityTypeBuilder<FranchiseOrderDetail> builder)
    {
                    builder.HasKey(e => e.Id).HasName("FranchiseOrderDetail_pkey");
        
                    builder.ToTable("FranchiseOrderDetail");
        
                    builder.HasIndex(e => e.FranchiseOrderId, "ix_franchiseorderdetail_orderid");
        
                    builder.HasIndex(e => e.ProductId, "ix_franchiseorderdetail_productid");
        
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.CommissionFee).HasColumnType("money");
                    builder.Property(e => e.UnitPrice).HasColumnType("money");
        
                    builder.HasOne(d => d.FranchiseOrder).WithMany(p => p.FranchiseOrderDetails)
                        .HasForeignKey(d => d.FranchiseOrderId)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FranchiseOrderDetail_FranchiseOrderId_fkey");
        
                    builder.HasOne(d => d.Product).WithMany(p => p.FranchiseOrderDetails)
                        .HasForeignKey(d => d.ProductId)
                        .HasConstraintName("FranchiseOrderDetail_ProductId_fkey");
    }
}

