using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class SupplierDebtConfiguration : IEntityTypeConfiguration<SupplierDebt>
{
    public void Configure(EntityTypeBuilder<SupplierDebt> builder)
    {
                    builder.HasKey(e => e.Id).HasName("SupplierDebt_pkey");
        
                    builder.ToTable("SupplierDebt");
        
                    builder.HasIndex(e => e.SupplierId, "uq_supplierdebt_supplier").IsUnique();
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.PaidAmount).HasColumnType("money");
                    builder.Property(e => e.RemainingDebt)
                        .HasComputedColumnSql("((\"TotalDebt\" - \"PaidAmount\") - \"ReturnAmount\")", true)
                        .HasColumnType("money");
                    builder.Property(e => e.ReturnAmount).HasColumnType("money");
                    builder.Property(e => e.TotalDebt).HasColumnType("money");
        
                    builder.HasOne(d => d.Supplier).WithOne(p => p.SupplierDebt)
                        .HasForeignKey<SupplierDebt>(d => d.SupplierId)
                        .HasConstraintName("SupplierDebt_SupplierId_fkey");
    }
}
