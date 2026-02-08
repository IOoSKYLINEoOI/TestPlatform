using TestPlatform.Infrastructure.Postgres.Entities;

namespace TestPlatform.Infrastructure.Postgres.Categories;

public class CategoryEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public ICollection<TestEntity> Tests { get; set; } = new List<TestEntity>();
}
