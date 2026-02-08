using TestPlatform.Core.Tests.Enums;

namespace TestPlatform.Infrastructure.Postgres.Entities;

public class QuestionEntity
{
    public int Id { get; set; }

    public string Text { get; set; } = null!;

    public int QuestionTypeId { get; set; }

    public QuestionType Type { get; set; }

    public int TestId { get; set; }

    public TestEntity Test { get; set; } = null!;

    public ICollection<AnswerEntity> Answers { get; set; } = new List<AnswerEntity>();
}
