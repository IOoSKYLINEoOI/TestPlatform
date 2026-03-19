using TestPlatform.Infrastructure.Postgres.Attempts.Entities;
using TestPlatform.Infrastructure.Postgres.Tests.Entities;

namespace TestPlatform.Infrastructure.Postgres.Users.Entities;

public class UserEntity
{
    public Guid Id { get; set; }

    public string KeycloakId { get; set; }

    public string TabNumber { get; set; }

    public ICollection<AttemptEntity> Attempts { get; set; } = new List<AttemptEntity>();

    public ICollection<TestEntity> Tests { get; set; } = new List<TestEntity>();
}