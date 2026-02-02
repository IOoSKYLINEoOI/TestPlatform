using TestPlatform.Infrastructure.Postgres.Categories;

namespace TestPlatform.Infrastructure.Postgres.Entities;

public class TestEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? TimeLimitSeconds { get; set; }

    public string Description { get; set; } = null!;

    public Guid? UserId { get; set; }

    public ICollection<QuestionEntity> Questions { get; set; } = new List<QuestionEntity>();

    public ICollection<TestAttemptEntity> TestAttempts { get; set; } = new List<TestAttemptEntity>();

    public ICollection<CategoryEntity> Categories { get; set; } = new List<CategoryEntity>();
}