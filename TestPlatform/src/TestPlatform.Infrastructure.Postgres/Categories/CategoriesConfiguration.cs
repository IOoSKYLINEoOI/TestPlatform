using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TestPlatform.Infrastructure.Postgres.Categories;

public class CategoriesConfiguration : IEntityTypeConfiguration<CategoryEntity>
{

    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(250)
            .IsRequired();

        builder.HasMany(x => x.Tests)
            .WithMany(x => x.Categories)
            .UsingEntity(x => x.ToTable("TestCategories"));
    }
}