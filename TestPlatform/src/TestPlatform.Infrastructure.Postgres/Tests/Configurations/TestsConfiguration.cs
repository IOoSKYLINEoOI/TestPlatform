using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Infrastructure.Postgres.Tests.Entities;

namespace TestPlatform.Infrastructure.Postgres.Tests.Configurations;

public class TestsConfiguration : IEntityTypeConfiguration<TestEntity>
{
    public void Configure(EntityTypeBuilder<TestEntity> builder)
    {
        builder.ToTable("Tests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(250)
            .IsRequired();

        builder.HasMany(x => x.Questions)
            .WithOne(x => x.Test)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.TestAttempts)
            .WithOne(x => x.Test)
            .OnDelete(DeleteBehavior.Cascade);
    }
}