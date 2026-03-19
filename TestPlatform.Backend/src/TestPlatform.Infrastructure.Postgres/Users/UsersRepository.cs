using CSharpFunctionalExtensions;
using TestPlatform.Application.Users;
using TestPlatform.Core.Users;
using TestPlatform.Infrastructure.Postgres.Users.Entities;

namespace TestPlatform.Infrastructure.Postgres.Users;

public class UsersRepository : IUsersRepository
{
    private readonly TestPlatformDbContext _context;

    public UsersRepository(TestPlatformDbContext context) => _context = context;

    public async Task<Result<Guid>> AddAsync(User user, CancellationToken cancellationToken)
    {
        var userEntity = MapToEntity(user);

        await _context.Users.AddAsync(userEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(userEntity.Id);
    }

    private UserEntity MapToEntity(User user)
        => new UserEntity() { Id = user.Id, KeycloakId = user.KeycloakId, TabNumber = user.TabNumber, };
}