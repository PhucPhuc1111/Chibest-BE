using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class ColorConfiguration : IEntityTypeConfiguration<Color>
{
    public void Configure(EntityTypeBuilder<Color> builder)
    {
                    builder.HasKey(e => e.Id).HasName("Color_pkey");
        
                    builder.ToTable("Color");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.Code).HasMaxLength(10);
    }
}
