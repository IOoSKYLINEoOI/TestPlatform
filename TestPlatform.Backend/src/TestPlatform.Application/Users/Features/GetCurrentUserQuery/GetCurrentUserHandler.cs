using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Users.DTOs;

namespace TestPlatform.Application.Users.Features.GetCurrentUserQuery;

public record GetCurrentUserQuery(string KeycloakId) : IQuery;

public class GetCurrentUserHandler : IQueryHandler<CurrentUserDto, GetCurrentUserQuery>
{
    private readonly IUsersReadRepository _usersReadRepository;
    private readonly ILogger<GetCurrentUserHandler> _logger;

    public GetCurrentUserHandler(IUsersReadRepository usersReadRepository, ILogger<GetCurrentUserHandler> logger)
    {
        _usersReadRepository = usersReadRepository;
        _logger = logger;
    }

    public async Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var user = await _usersReadRepository.GetByKeycloakIdAsync(query.KeycloakId,  cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("CurrentUser with KeycloakId {Id} not found", query.KeycloakId);
            return Result.Failure<CurrentUserDto>("Question not found");
        }

        _logger.LogInformation("Get CurrentUser with KeycloakId {Id}", query.KeycloakId);
        return Result.Success(user);
    }
}