namespace TestPlatform.Infrastructure.Postgres.Entities;

public class TestAttemptEntity
{

    public int Id { get; set; }

    public int TotalQuestions { get; set; }

    public int CorrectAnswers { get; set; }

    public double Score { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public int TestId { get; set; }

    public TestEntity Test { get; set; } = null!;
}