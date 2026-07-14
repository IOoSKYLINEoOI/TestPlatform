using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestPlatform.Core.Attempts;

namespace TestPlatform.Infrastructure.Postgres.Attempts.Configurations;

public class AttemptsConfiguration : IEntityTypeConfiguration<Attempt>
{
    public void Configure(EntityTypeBuilder<Attempt> builder)
    {
        builder.ToTable("attempts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.SourceId).IsRequired();

        builder.Property(x => x.TotalQuestions).IsRequired();

        builder.Property(x => x.TotalMaxScore)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.TimeLimitSeconds);

        builder.Property(x => x.Deadline);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.StartedAt);
        builder.Property(x => x.FinishedAt);

        builder.Ignore(x => x.Score);

        builder.OwnsOne(x => x.AttemptResult, b =>
        {
            b.ToJson();
        });

        builder.Navigation(x => x.AttemptAnswers)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.AttemptAnswers, b =>
        {
            b.ToTable("attempt_answers");

            b.WithOwner()
                .HasForeignKey("AttemptId");

            b.HasKey("AttemptId", nameof(AttemptAnswer.QuestionId));

            b.Property(x => x.QuestionId).IsRequired();

            b.Property(x => x.TextAnswer);

            b.Property(x => x.NumberAnswer)
                .HasPrecision(10, 2);

            b.Property(x => x.SelectedOptionIds)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions)null!)!
                );

            b.Property(x => x.MatchingPairs)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<AttemptMatchingPair>>(v, (JsonSerializerOptions)null!)!
                );

            b.HasIndex("AttemptId", nameof(AttemptAnswer.QuestionId))
                .IsUnique();
        });

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.Status });
        builder.HasIndex(x => new { x.Type, x.SourceId });
    }
}