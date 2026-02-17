using TestPlatform.Core.Tests.Enums;

namespace TestPlatform.Infrastructure.Postgres.Tests.Entities;

public class QuestionEntity
{
    public Guid Id { get; set; }

    public string Text { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public int QuestionTypeId { get; set; }

    public QuestionType Type { get; set; }

    public Guid TestId { get; set; }

    public TestEntity Test { get; set; } = null!;

    public ICollection<AnswerEntity> Answers { get; set; } = new List<AnswerEntity>();
}
