using TestPlatform.Core.TestAttempts.Enums;
using TestPlatform.Infrastructure.Postgres.Exams.Entities;
using TestPlatform.Infrastructure.Postgres.Tests.Entities;

namespace TestPlatform.Infrastructure.Postgres.Attempts.Entities;

public class AttemptEntity
{
    public Guid Id { get; set; }

    public int TotalQuestions { get; set; }

    public int CorrectAnswers { get; set; }

    public double Score { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public Guid ParentId { get; set; }

    public AttemptParentType ParentType { get; set; }
}