using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Users.Features.GetCurrentUserQuery;
using TestPlatform.Contracts.Users.DTOs;

namespace TestPlatform.Presenters.Users;

[ApiController]
[Route("users")]
[Authorize]
public class UsersController : ControllerBase
{
    [HttpGet("me")]
    [SwaggerOperation(
        OperationId = "GetCurrentUser",
        Summary = "Получить текущего пользователя",
        Description = "Возвращает текущего пользователя")]
    public async Task<IActionResult> GetCurrentUser(
        [FromServices] IQueryHandler<CurrentUserResponse, GetCurrentUserQuery> handler,
        CancellationToken cancellationToken)
    {
        var keycloakId =
            User.FindFirst("sub")?.Value ??
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(keycloakId))
            return Unauthorized();

        var query = new GetCurrentUserQuery(keycloakId);

        var currentUser = await handler.Handle(query, cancellationToken);
        if (currentUser == null)
            return Problem("User not found. Middleware failure.");

        return Ok(currentUser);
    }

    [HttpGet("me2")]
    public IActionResult Me2()
    {
        var claims = User.Claims.ToDictionary(c => c.Type, c => c.Value);
        return Ok(claims);
    }
}