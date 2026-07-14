using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Users;

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
    public IActionResult GetCurrentUser(CancellationToken cancellationToken)
    {
        var currentUser = _currentUserAccessor.User;

        if (currentUser == null)
            return Unauthorized();

        return Ok(currentUser);
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