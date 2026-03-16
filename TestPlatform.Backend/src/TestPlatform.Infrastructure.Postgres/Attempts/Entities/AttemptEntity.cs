using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Infrastructure.Postgres.Attempts.Entities;

public class AttemptEntity
{
    public Guid Id { get; set; }

    public int TotalQuestions { get; set; }

    public int? CorrectAnswers { get; set; }

    public decimal EarnedPoints { get; set; }

    public decimal MaxPoints { get; set; }

    public Guid? UserId { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public AttemptStatus Status { get; set; }

    public AttemptType Type { get; set; }

    public Guid SourceId { get; set; }
}