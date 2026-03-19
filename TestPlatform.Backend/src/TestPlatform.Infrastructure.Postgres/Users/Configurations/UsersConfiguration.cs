using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Infrastructure.Postgres.Users.Entities;

namespace TestPlatform.Infrastructure.Postgres.Users.Configurations;

public class UsersConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.KeycloakId)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.TabNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasMany(x => x.Attempts)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Tests)
            .WithOne(x => x.Author)
            .HasForeignKey(x => x.AuthorId);
    }
}