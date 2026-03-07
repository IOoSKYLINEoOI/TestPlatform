using TestPlatform.Infrastructure.Postgres.Attempts.Entities;
using TestPlatform.Infrastructure.Postgres.Questions.Entities;

namespace TestPlatform.Infrastructure.Postgres.Exams.Entities;

public class ExamEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int? TimeLimitSeconds { get; set; }

    public ICollection<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();
}