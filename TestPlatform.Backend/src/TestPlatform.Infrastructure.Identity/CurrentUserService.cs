using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Users;

namespace TestPlatform.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUsersReadRepository _usersRepository;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        IUsersReadRepository usersRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _usersRepository = usersRepository;
    }

    public string? KeycloakId =>
        _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;

    public async Task<Result<Guid>> GetUserIdAsync(CancellationToken cancellationToken)
    {
        if (KeycloakId is null)
            return Result.Failure<Guid>("unauthorized");

        var user = await _usersRepository.GetByKeycloakIdAsync(KeycloakId, cancellationToken);

        if (user == null)
            return Result.Failure<Guid>("user.not_found");

        return Result.Success(user.Id);
    }
}