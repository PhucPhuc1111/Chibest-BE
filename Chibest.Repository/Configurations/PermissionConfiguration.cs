using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
                    builder.HasKey(e => e.Id).HasName("Permission_pkey");
        
                    builder.ToTable("Permission");
        
                    builder.HasIndex(e => e.Code, "Permission_Code_key").IsUnique();
        
                    builder.HasIndex(e => e.Code, "ix_permission_code");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.Code).HasMaxLength(100);
    }
}
