using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class AccountRoleConfiguration : IEntityTypeConfiguration<AccountRole>
{
    public void Configure(EntityTypeBuilder<AccountRole> builder)
    {
                    builder.HasKey(e => new { e.AccountId, e.RoleId }).HasName("pk_accountrole");
        
                    builder.ToTable("AccountRole");
        
                    builder.HasIndex(e => e.AccountId, "ix_accountrole_accountid");
        
                    builder.HasIndex(e => new { e.BranchId, e.RoleId }, "ix_accountrole_branchid_roleid");
        
                    builder.HasOne(d => d.Account).WithMany(p => p.AccountRoles)
                        .HasForeignKey(d => d.AccountId)
                        .HasConstraintName("fk_accountrole_account");
        
                    builder.HasOne(d => d.Branch).WithMany(p => p.AccountRoles)
                        .HasForeignKey(d => d.BranchId)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("fk_accountrole_branch");
        
                    builder.HasOne(d => d.Role).WithMany(p => p.AccountRoles)
                        .HasForeignKey(d => d.RoleId)
                        .HasConstraintName("fk_accountrole_role");
    }
}
