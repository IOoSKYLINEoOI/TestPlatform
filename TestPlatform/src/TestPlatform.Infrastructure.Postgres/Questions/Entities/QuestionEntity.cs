using TestPlatform.Core.Questions;
using TestPlatform.Infrastructure.Postgres.Exams.Entities;
using TestPlatform.Infrastructure.Postgres.Tags;
using TestPlatform.Infrastructure.Postgres.Tags.Entities;
using TestPlatform.Infrastructure.Postgres.Tests.Entities;

namespace TestPlatform.Infrastructure.Postgres.Questions.Entities;

public class QuestionEntity
{
    public Guid Id { get; set; }

    public string Text { get; set; } = null!;

    public int QuestionTypeId { get; set; }

    public int Points { get; set; }

    public string? ImageUrl { get; set; }

    public ICollection<AnswerOptionEntity> Answers { get; set; } = new List<AnswerOptionEntity>();

    public ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();

    public ICollection<TestEntity> Tests { get; set; } = new List<TestEntity>();

    public ICollection<ExamEntity> Exams { get; set; } = new List<ExamEntity>();
}
