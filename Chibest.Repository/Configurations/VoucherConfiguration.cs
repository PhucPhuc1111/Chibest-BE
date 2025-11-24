using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
                    builder.HasKey(e => e.Id).HasName("Voucher_pkey");
        
                    builder.ToTable("Voucher");
        
                    builder.HasIndex(e => e.Code, "Voucher_Code_key").IsUnique();
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.AvailableDate)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.Code).HasMaxLength(100);
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.DiscountAmount)
                        .HasDefaultValueSql("0")
                        .HasColumnType("money");
                    builder.Property(e => e.DiscountPercent)
                        .HasPrecision(5, 2)
                        .HasDefaultValueSql("0");
                    builder.Property(e => e.ExpiredDate).HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.MaxDiscountAmount).HasColumnType("money");
                    builder.Property(e => e.MinimumTransaction).HasColumnType("money");
                    builder.Property(e => e.Name).HasMaxLength(250);
                    builder.Property(e => e.Status)
                        .HasMaxLength(40)
                        .HasDefaultValueSql("'Available'::character varying");
                    builder.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.UsagePerCustomer).HasDefaultValue(1);
                    builder.Property(e => e.UsedCount).HasDefaultValue(0);
                    builder.Property(e => e.VoucherType)
                        .HasMaxLength(50)
                        .HasDefaultValueSql("'Giảm Giá'::character varying");
    }
}
