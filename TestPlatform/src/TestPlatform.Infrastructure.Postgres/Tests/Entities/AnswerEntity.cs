namespace TestPlatform.Infrastructure.Postgres.Tests.Entities;

public class AnswerEntity
{
    public Guid Id { get; set; }

    public string Text { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public string? ImageUrl { get; set; }

    public Guid QuestionId { get; set; }

    public QuestionEntity Question { get; set; } = null!;
}