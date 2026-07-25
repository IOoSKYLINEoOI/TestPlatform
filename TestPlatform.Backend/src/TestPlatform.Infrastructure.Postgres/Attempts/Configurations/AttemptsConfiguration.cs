using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
        builder.Property(x => x.RequestId).IsRequired();

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.SourceId).IsRequired();
        builder.Property(x => x.AttemptNumber).IsRequired();

        builder.Property(x => x.TotalQuestions).IsRequired();

        builder.Property(x => x.TotalMaxScore)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.TimeLimitSeconds);
        builder.Property(x => x.MinPassingScore).HasPrecision(10, 2);
        builder.Property(x => x.MinPassingPercent);
        builder.Property(x => x.LatestFinishAt);
        builder.Property(x => x.ReviewAvailableAt);

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

        builder.Navigation(x => x.QuestionSelections)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(x => x.QuestionSelections, b =>
        {
            b.ToTable("attempt_questions");

            b.WithOwner()
                .HasForeignKey("AttemptId");

            b.HasKey("AttemptId", nameof(AttemptQuestionSelection.QuestionId));
            b.Property(x => x.QuestionId).IsRequired();
            b.HasOne<TestPlatform.Core.Questions.Question>()
                .WithMany()
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Property(x => x.Order).IsRequired();
            b.Property(x => x.Score).HasPrecision(10, 2).IsRequired();
        });

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

            var selectedOptionIds = b.Property(x => x.SelectedOptionIds);
            selectedOptionIds
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<Guid>>(v, (JsonSerializerOptions)null!)!);
            selectedOptionIds.Metadata.SetValueComparer(
                new ValueComparer<IReadOnlyCollection<Guid>>(
                    (left, right) => left!.SequenceEqual(right!),
                    value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
                    value => value.ToList().AsReadOnly()));

            var matchingPairs = b.Property(x => x.MatchingPairs);
            matchingPairs
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                    v => JsonSerializer.Deserialize<List<AttemptMatchingPair>>(v, (JsonSerializerOptions)null!)!);
            matchingPairs.Metadata.SetValueComparer(
                new ValueComparer<IReadOnlyCollection<AttemptMatchingPair>>(
                    (left, right) => left!.SequenceEqual(right!),
                    value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
                    value => value.ToList().AsReadOnly()));

            b.HasIndex("AttemptId", nameof(AttemptAnswer.QuestionId))
                .IsUnique();
        });

        builder.HasIndex(x => new { x.UserId, x.StartedAt });
        builder.HasIndex(x => new { x.Type, x.SourceId, x.StartedAt });
        builder.HasIndex(x => new { x.UserId, x.Type, x.SourceId, x.AttemptNumber }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.RequestId }).IsUnique();
    }
}
