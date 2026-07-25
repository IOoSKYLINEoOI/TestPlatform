using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Api.Common;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Users;
using TestPlatform.Application.Users.Features;
using TestPlatform.Contracts.Authorization;
using TestPlatform.Contracts.Users.DTOs;

namespace TestPlatform.Api.Users;

[ApiController]
[Route("users")]
[Authorize]
public class UsersController(
    ICurrentUserAccessor currentUserAccessor) : ApiControllerBase
{
    [HttpGet("me")]
    [SwaggerOperation(
        OperationId = "GetCurrentUser",
        Summary = "Получить данные текущего пользователя",
        Description = "Возвращает данные пользователя, полученный из аутентифицированного токена.")]
    public IActionResult GetCurrentUser()
    {
        var currentUser = currentUserAccessor.User;

        return currentUser is null
            ? Unauthorized()
            : Ok(new CurrentUserResponse(
                currentUser.Id,
                currentUser.EmployeeNumber,
                currentUser.IsAdmin));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ManageSystem)]
    [SwaggerOperation(
        OperationId = "CreateUserAccount",
        Summary = "Create an employee account",
        Description = "Creates a Keycloak account with an employee number, temporary password, and realm role.")]
    [ProducesResponseType(typeof(CreateUserAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<CreateUserAccountCommand, CreateUserAccountResponse> handler,
        [FromBody] CreateUserAccountRequest request,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new CreateUserAccountCommand(request),
            cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ToErrorResult(result.Error);
    }
}
