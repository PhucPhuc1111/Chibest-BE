using System.Collections.Generic;
using Chibest.Repository.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chibest.Repository.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
                    builder.HasKey(e => e.Id).HasName("Category_pkey");
        
                    builder.ToTable("Category");
        
                    builder.HasIndex(e => e.Name, "ix_category_type_name");
        
                    builder.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
                    builder.Property(e => e.Name).HasMaxLength(150);
    }
}
