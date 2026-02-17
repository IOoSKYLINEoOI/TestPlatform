using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Infrastructure.Postgres.Tests.Entities;

namespace TestPlatform.Infrastructure.Postgres.Tests.Configurations;

public class AnswersConfiguration : IEntityTypeConfiguration<AnswerEntity>
{
    public void Configure(EntityTypeBuilder<AnswerEntity> builder)
    {
        builder.ToTable("Answers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.IsCorrect)
            .IsRequired();
    }
}