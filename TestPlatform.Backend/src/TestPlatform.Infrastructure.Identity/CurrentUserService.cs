using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Users;

namespace TestPlatform.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUsersReadDbContext _usersReadDbContext;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IUsersReadDbContext usersReadDbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _usersReadDbContext = usersReadDbContext;
    }

    public string? KeycloakId =>
        _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

    public async Task<Result<Guid>> GetUserIdAsync(CancellationToken cancellationToken)
    {
        if (KeycloakId is null)
            return Result.Failure<Guid>("unauthorized");

        var userId = await _usersReadDbContext.ReadUsers
            .Where(x => x.KeycloakId == KeycloakId)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return userId == Guid.Empty ? Result.Failure<Guid>("user.not_found") : Result.Success(userId);
    }
}