using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Core.Users;

namespace TestPlatform.Infrastructure.Postgres.Users.Configurations;

public class UsersConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.KeycloakId)
            .HasMaxLength(User.MaxKeycloakIdLength)
            .IsRequired();

        builder.Property(x => x.EmployeeNumber)
            .HasMaxLength(User.MaxEmployeeNumberLength)
            .IsRequired();

        builder.HasIndex(x => x.KeycloakId)
            .IsUnique();

        builder.HasIndex(x => x.EmployeeNumber)
            .IsUnique();
    }
}
