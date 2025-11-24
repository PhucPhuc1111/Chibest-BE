using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class StockAdjustmentDetailConfiguration : IEntityTypeConfiguration<StockAdjustmentDetail>
{
    public void Configure(EntityTypeBuilder<StockAdjustmentDetail> builder)
    {
                    builder.HasKey(e => e.Id).HasName("StockAdjustmentDetail_pkey");
        
                    builder.ToTable("StockAdjustmentDetail");
        
                    builder.HasIndex(e => e.StockAdjustmentId, "ix_stockadjustmentdetail_adjustmentid");
        
                    builder.HasIndex(e => e.ProductId, "ix_stockadjustmentdetail_productid");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.DifferenceQty).HasComputedColumnSql("(\"ActualQty\" - \"SystemQty\")", true);
                    builder.Property(e => e.TotalValueChange)
                        .HasComputedColumnSql("((\"ActualQty\" - \"SystemQty\") * \"UnitCost\")", true)
                        .HasColumnType("money");
                    builder.Property(e => e.UnitCost).HasColumnType("money");
        
                    builder.HasOne(d => d.Product).WithMany(p => p.StockAdjustmentDetails)
                        .HasForeignKey(d => d.ProductId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("StockAdjustmentDetail_ProductId_fkey");
        
                    builder.HasOne(d => d.StockAdjustment).WithMany(p => p.StockAdjustmentDetails)
                        .HasForeignKey(d => d.StockAdjustmentId)
                        .HasConstraintName("StockAdjustmentDetail_StockAdjustmentId_fkey");
    }
}
