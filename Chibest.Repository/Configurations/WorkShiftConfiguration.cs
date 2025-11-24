using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class WorkShiftConfiguration : IEntityTypeConfiguration<WorkShift>
{
    public void Configure(EntityTypeBuilder<WorkShift> builder)
    {
                    builder.HasKey(e => e.Id).HasName("WorkShift_pkey");
        
                    builder.ToTable("WorkShift");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.IsOvernight).HasDefaultValue(false);
                    builder.Property(e => e.Name).HasMaxLength(100);
                    builder.Property(e => e.ShiftCoefficient)
                        .HasPrecision(5, 2)
                        .HasDefaultValueSql("1.0");
    }
}
