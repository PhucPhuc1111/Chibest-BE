using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
                    builder.HasKey(e => e.Id).HasName("Branch_pkey");
        
                    builder.ToTable("Branch");
        
                    builder.HasIndex(e => e.Code, "Branch_Code_key").IsUnique();
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.Address).HasMaxLength(500);
                    builder.Property(e => e.Code).HasMaxLength(50);
                    builder.Property(e => e.CreatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.IsFranchise).HasDefaultValue(false);
                    builder.Property(e => e.Name).HasMaxLength(255);
                    builder.Property(e => e.PhoneNumber).HasMaxLength(15);
                    builder.Property(e => e.Status)
                        .HasMaxLength(40)
                        .HasDefaultValueSql("'Active'::character varying");
                    builder.Property(e => e.UpdatedAt)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
    }
}
