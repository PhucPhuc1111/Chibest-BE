using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class CustomerVoucherConfiguration : IEntityTypeConfiguration<CustomerVoucher>
{
    public void Configure(EntityTypeBuilder<CustomerVoucher> builder)
    {
                    builder.HasKey(e => new { e.VoucherId, e.CustomerId }).HasName("CustomerVoucher_pkey");
        
                    builder.ToTable("CustomerVoucher");
        
                    builder.Property(e => e.CollectedDate)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.Status)
                        .HasMaxLength(40)
                        .HasDefaultValueSql("'Đã Nhận'::character varying");
                    builder.Property(e => e.UsedDate).HasColumnType("timestamp(3) without time zone");
        
                    builder.HasOne(d => d.Customer).WithMany(p => p.CustomerVouchers)
                        .HasForeignKey(d => d.CustomerId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("CustomerVoucher_CustomerId_fkey");
        
                    builder.HasOne(d => d.Voucher).WithMany(p => p.CustomerVouchers)
                        .HasForeignKey(d => d.VoucherId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("CustomerVoucher_VoucherId_fkey");
    }
}
