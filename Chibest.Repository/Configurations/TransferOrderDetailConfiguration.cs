using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class TransferOrderDetailConfiguration : IEntityTypeConfiguration<TransferOrderDetail>
{
    public void Configure(EntityTypeBuilder<TransferOrderDetail> builder)
    {
                    builder.HasKey(e => e.Id).HasName("TransferOrderDetail_pkey");
        
                    builder.ToTable("TransferOrderDetail");
        
                    builder.HasIndex(e => e.TransferOrderId, "ix_transferorderdetail_orderid");
        
                    builder.HasIndex(e => e.ProductId, "ix_transferorderdetail_productid");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.CommissionFee).HasColumnType("money");
                    builder.Property(e => e.UnitPrice).HasColumnType("money");
        
                    builder.HasOne(d => d.Product).WithMany(p => p.TransferOrderDetails)
                        .HasForeignKey(d => d.ProductId)
                        .HasConstraintName("TransferOrderDetail_ProductId_fkey");
        
                    builder.HasOne(d => d.TransferOrder).WithMany(p => p.TransferOrderDetails)
                        .HasForeignKey(d => d.TransferOrderId)
                        .HasConstraintName("TransferOrderDetail_TransferOrderId_fkey");
    }
}
