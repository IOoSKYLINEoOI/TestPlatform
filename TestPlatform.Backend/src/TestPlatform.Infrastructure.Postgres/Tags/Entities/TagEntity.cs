using TestPlatform.Infrastructure.Postgres.Questions.Entities;

namespace TestPlatform.Infrastructure.Postgres.Tags.Entities;

public class TagEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public ICollection<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();
}
