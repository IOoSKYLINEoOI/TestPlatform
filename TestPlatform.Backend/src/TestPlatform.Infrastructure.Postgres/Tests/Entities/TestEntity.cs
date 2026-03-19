using TestPlatform.Infrastructure.Postgres.Questions.Entities;
using TestPlatform.Infrastructure.Postgres.Users.Entities;

namespace TestPlatform.Infrastructure.Postgres.Tests.Entities;

public class TestEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int? TimeLimitSeconds { get; set; }

    public Guid? AuthorId { get; set; }

    public UserEntity? Author { get; set; }

    public string? CoverImageName { get; set; } = null!;

    public ICollection<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();
}