/*using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Contracts.Users.DTOs;

namespace TestPlatform.Application.Users.Features.GetCurrentUserQuery;

public record GetCurrentUserQuery(string KeycloakId) : IQuery;

public class GetCurrentUserHandler(IUsersReadDbContext usersDbContext)
    : IQueryHandler<GetCurrentUserQuery, CurrentUserDto>
{
    public async Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        var response = await usersDbContext.ReadUsers
            .AsNoTracking()
            .Where(u => u.KeycloakId == query.KeycloakId)
            .Select(u => new CurrentUserDto(
                u.Id,
                u.KeycloakId,
                u.TabNumber,
                u.))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return response is null
            ? Result.Failure<CurrentUserDto>(ErrorCodes.Forbidden)
            : Result.Success(response);
    }
}*/