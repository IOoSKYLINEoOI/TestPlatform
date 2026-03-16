namespace TestPlatform.Infrastructure.Postgres.Questions.Entities;

public class AnswerOptionEntity
{
    public Guid Id { get; set; }

    public string Text { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public string? ImageName { get; set; }

    public Guid QuestionId { get; set; }

    public QuestionEntity Question { get; set; } = null!;
}