using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class BranchDebtConfiguration : IEntityTypeConfiguration<BranchDebt>
{
    public void Configure(EntityTypeBuilder<BranchDebt> builder)
    {
                    builder.HasKey(e => e.Id).HasName("BranchDebt_pkey");
        
                    builder.ToTable("BranchDebt");
        
                    builder.HasIndex(e => e.BranchId, "uq_branchdebt_branch").IsUnique();
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.LastTransactionDate).HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.LastUpdated)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.PaidAmount).HasColumnType("money");
                    builder.Property(e => e.RemainingDebt)
                        .HasComputedColumnSql("((\"TotalDebt\" - \"PaidAmount\") - \"ReturnAmount\")", true)
                        .HasColumnType("money");
                    builder.Property(e => e.ReturnAmount).HasColumnType("money");
                    builder.Property(e => e.TotalDebt).HasColumnType("money");
        
                    builder.HasOne(d => d.Branch).WithOne(p => p.BranchDebt)
                        .HasForeignKey<BranchDebt>(d => d.BranchId)
                        .HasConstraintName("BranchDebt_BranchId_fkey");
    }
}
