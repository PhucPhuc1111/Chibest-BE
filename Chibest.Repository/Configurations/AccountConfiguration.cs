using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
                    builder.HasKey(e => e.Id).HasName("Account_pkey");
        
                    builder.ToTable("Account");
        
                    builder.HasIndex(e => e.Code, "Account_Code_key").IsUnique();
        
                    builder.HasIndex(e => e.Email, "Account_Email_key").IsUnique();
        
                    builder.HasIndex(e => e.Email, "ix_account_email");
        
                    builder.HasIndex(e => e.PhoneNumber, "ix_account_phonenumber");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.AvatarUrl).HasColumnName("AvatarURL");
                    builder.Property(e => e.Code).HasMaxLength(100);
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.Email).HasMaxLength(100);
                    builder.Property(e => e.FcmToken).HasMaxLength(255);
                    builder.Property(e => e.Name).HasMaxLength(250);
                    builder.Property(e => e.PhoneNumber).HasMaxLength(15);
                    builder.Property(e => e.RefreshTokenExpiryTime).HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.Status)
                        .HasMaxLength(40)
                        .HasDefaultValueSql("'Active'::character varying");
                    builder.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
    }
}
