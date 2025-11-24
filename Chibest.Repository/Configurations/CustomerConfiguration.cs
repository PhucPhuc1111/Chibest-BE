using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
                    builder.HasKey(e => e.Id).HasName("Customer_pkey");
        
                    builder.ToTable("Customer");
        
                    builder.HasIndex(e => e.Code, "Customer_Code_key").IsUnique();
        
                    builder.HasIndex(e => e.Email, "ix_customer_email");
        
                    builder.HasIndex(e => e.GroupId, "ix_customer_groupid");
        
                    builder.HasIndex(e => e.PhoneNumber, "ix_customer_phonenumber");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.AvatarUrl).HasColumnName("AvatarURL");
                    builder.Property(e => e.Code).HasMaxLength(20);
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.DateOfBirth).HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.Email).HasMaxLength(100);
                    builder.Property(e => e.LastActive)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.Name).HasMaxLength(200);
                    builder.Property(e => e.PhoneNumber).HasMaxLength(15);
                    builder.Property(e => e.Status)
                        .HasMaxLength(30)
                        .HasDefaultValueSql("'Working'::character varying");
                    builder.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
        
                    builder.HasOne(d => d.Group).WithMany(p => p.InverseGroup)
                        .HasForeignKey(d => d.GroupId)
                        .HasConstraintName("Customer_GroupId_fkey");
    }
}
