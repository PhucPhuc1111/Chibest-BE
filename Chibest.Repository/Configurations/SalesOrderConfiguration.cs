using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
                    builder.HasKey(e => e.Id).HasName("SalesOrder_pkey");
        
                    builder.ToTable("SalesOrder");
        
                    builder.HasIndex(e => e.OrderCode, "SalesOrder_OrderCode_key").IsUnique();
        
                    builder.HasIndex(e => new { e.BranchId, e.OrderDate }, "ix_salesorder_branchid").IsDescending(false, true);
        
                    builder.HasIndex(e => new { e.CustomerId, e.OrderDate }, "ix_salesorder_customerid").IsDescending(false, true);
        
                    builder.HasIndex(e => e.PaymentStatus, "ix_salesorder_paymentstatus");
        
                    builder.HasIndex(e => new { e.Status, e.OrderDate }, "ix_salesorder_status").IsDescending(false, true);
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.ActualDeliveryDate).HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.CustomerEmail).HasMaxLength(100);
                    builder.Property(e => e.CustomerName).HasMaxLength(250);
                    builder.Property(e => e.CustomerPhone).HasMaxLength(15);
                    builder.Property(e => e.DeliveryMethod)
                        .HasMaxLength(50)
                        .HasDefaultValueSql("'Tại Cửa Hàng'::character varying");
                    builder.Property(e => e.DiscountAmount).HasColumnType("money");
                    builder.Property(e => e.ExpectedDeliveryDate).HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.FinalAmount)
                        .HasComputedColumnSql("(((\"SubTotal\" - \"DiscountAmount\") - \"VoucherAmount\") + \"ShippingFee\")", true)
                        .HasColumnType("money");
                    builder.Property(e => e.OrderCode).HasMaxLength(100);
                    builder.Property(e => e.OrderDate)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.PaidAmount).HasColumnType("money");
                    builder.Property(e => e.PaymentMethod)
                        .HasMaxLength(50)
                        .HasDefaultValueSql("'Tiền Mặt'::character varying");
                    builder.Property(e => e.PaymentStatus)
                        .HasMaxLength(50)
                        .HasDefaultValueSql("'Chờ Thanh Toán'::character varying");
                    builder.Property(e => e.ShippingAddress).HasMaxLength(500);
                    builder.Property(e => e.ShippingFee).HasColumnType("money");
                    builder.Property(e => e.ShippingPhone).HasMaxLength(15);
                    builder.Property(e => e.Status)
                        .HasMaxLength(50)
                        .HasDefaultValueSql("'Đặt Trước'::character varying");
                    builder.Property(e => e.SubTotal).HasColumnType("money");
                    builder.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.VoucherAmount).HasColumnType("money");
        
                    builder.HasOne(d => d.Branch).WithMany(p => p.SalesOrders)
                        .HasForeignKey(d => d.BranchId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("SalesOrder_BranchId_fkey");
        
                    builder.HasOne(d => d.Customer).WithMany(p => p.SalesOrders)
                        .HasForeignKey(d => d.CustomerId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("SalesOrder_CustomerId_fkey");
        
                    builder.HasOne(d => d.Employee).WithMany(p => p.SalesOrders)
                        .HasForeignKey(d => d.EmployeeId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("SalesOrder_EmployeeId_fkey");
        
                    builder.HasOne(d => d.Voucher).WithMany(p => p.SalesOrders)
                        .HasForeignKey(d => d.VoucherId)
                        .HasConstraintName("SalesOrder_VoucherId_fkey");
    }
}
