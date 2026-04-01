using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Users;
using TestPlatform.Application.Users.Features.GetCurrentUserQuery;
using TestPlatform.Contracts.Users.DTOs;

namespace TestPlatform.Presenters.Users;

[ApiController]
[Route("users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public UsersController(ICurrentUserAccessor currentUserAccessor)
    {
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpGet("me")]
    [SwaggerOperation(
        OperationId = "GetCurrentUser",
        Summary = "Получить текущего пользователя",
        Description = "Возвращает текущего пользователя")]
    public async Task<IActionResult> GetCurrentUser(
        [FromServices] IQueryHandler<CurrentUserDto, GetCurrentUserQuery> handler,
        CancellationToken cancellationToken)
    {
        var keycloakId = _currentUserAccessor.User?.KeycloakId;

        if (string.IsNullOrEmpty(keycloakId))
            return Unauthorized();

        var query = new GetCurrentUserQuery(keycloakId);

        var result = await handler.Handle(query, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
    }

    [HttpGet("me2")]
    public IActionResult Me2()
    {
        var claims = User.Claims
            .GroupBy(c => c.Type)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Value).ToList());
        return Ok(claims);
    }
}