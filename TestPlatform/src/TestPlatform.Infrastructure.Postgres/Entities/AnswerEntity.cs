namespace TestPlatform.Infrastructure.Postgres.Entities;

public class AnswerEntity
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public int QuestionId { get; set; }

    public QuestionEntity Question { get; set; } = null!;
} 