using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class FranchiseInvoiceConfiguration : IEntityTypeConfiguration<FranchiseInvoice>
{
    public void Configure(EntityTypeBuilder<FranchiseInvoice> builder)
    {
                    builder.HasKey(e => e.Id).HasName("FranchiseInvoice_pkey");
        
                    builder.ToTable("FranchiseInvoice");
        
                    builder.HasIndex(e => e.Code, "FranchiseInvoice_Code_key").IsUnique();
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.Code).HasMaxLength(100);
                    builder.Property(e => e.OrderDate)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP")
                        .HasColumnType("timestamp(3) without time zone");
                    builder.Property(e => e.Status)
                        .HasMaxLength(40)
                        .HasDefaultValueSql("'Draft'::character varying");
                    builder.Property(e => e.TotalMoney).HasColumnType("money");
        
                    builder.HasOne(d => d.Branch).WithMany(p => p.FranchiseInvoices)
                        .HasForeignKey(d => d.BranchId)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasConstraintName("FranchiseInvoice_BranchId_fkey");
    }
}

