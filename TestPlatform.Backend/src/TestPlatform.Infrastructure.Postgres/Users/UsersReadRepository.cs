using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Users.DTOs;

namespace TestPlatform.Infrastructure.Postgres.Users;

public class UsersReadRepository : IUsersReadRepository
{
    private readonly TestPlatformDbContext _context;

    public UsersReadRepository(TestPlatformDbContext context) => _context = context;

    public async Task<CurrentUserResponse?> GetByKeycloakIdAsync(string keycloakId, CancellationToken cancellationToken)
        => await _context.Users
            .AsNoTracking()
            .Where(x => x.KeycloakId == keycloakId)
            .Select(x => new CurrentUserResponse(
                Id: x.Id,
                KeycloakId: x.KeycloakId,
                TabNumber: x.TabNumber))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<bool> ExistsAsync(string keycloakId, CancellationToken cancellationToken)
        => await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.KeycloakId == keycloakId, cancellationToken);
}