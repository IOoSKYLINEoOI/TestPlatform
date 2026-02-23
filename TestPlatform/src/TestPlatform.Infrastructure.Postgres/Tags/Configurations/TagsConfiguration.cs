using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Infrastructure.Postgres.Tags.Entities;

namespace TestPlatform.Infrastructure.Postgres.Tags.Configurations;

public class TagsConfiguration : IEntityTypeConfiguration<TagEntity>
{
    public void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        builder.ToTable("tags");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Description)
            .HasMaxLength(250);
    }
}