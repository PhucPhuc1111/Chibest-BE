using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
                    builder.HasKey(e => e.Id).HasName("Role_pkey");
        
                    builder.ToTable("Role");
        
                    builder.HasIndex(e => e.Name, "ix_role_name");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.Name).HasMaxLength(200);
        
                    builder.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                        .UsingEntity<Dictionary<string, object>>(
                            "RolePermission",
                            r => r.HasOne<Permission>().WithMany()
                                .HasForeignKey("PermissionId")
                                .HasConstraintName("fk_rolepermission_permission"),
                            l => l.HasOne<Role>().WithMany()
                                .HasForeignKey("RoleId")
                                .HasConstraintName("fk_rolepermission_role"),
                            j =>
                            {
                                j.HasKey("RoleId", "PermissionId").HasName("pk_rolepermission");
                                j.ToTable("RolePermission");
                            });
    }
}
