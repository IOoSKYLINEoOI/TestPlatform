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

    public async Task<CurrentUserDto?> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var user = await _usersReadRepository.GetByKeycloakIdAsync(query.KeycloakId,  cancellationToken);

        if(user == null)
            _logger.LogError("User with KeycloadId {Id} not found", query.KeycloakId);
        else
            _logger.LogInformation("Get User with KeycloadId {Id}", query.KeycloakId);

        return user;
    }
}