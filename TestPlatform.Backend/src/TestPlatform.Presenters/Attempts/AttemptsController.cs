using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Features.FinishAttemptCommand;
using TestPlatform.Application.Attempts.Features.GetByIdAttemptQuery;
using TestPlatform.Application.Attempts.Features.StartAttemptCommand;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Attempts.DTOs;

namespace TestPlatform.Presenters.Attempts;

[ApiController]
[Route("attempts")]
public class AttemptsController : ControllerBase
{
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public AttemptsController(ICurrentUserAccessor currentUserAccessor)
    {
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        OperationId = "GetByIdAttempt",
        Summary = "Получить результаты попытки по Id.",
        Description = "Возвращает результат попытки по его Id")]
    public async Task<IActionResult> GetById(
        [FromServices] IQueryHandler<AttemptResponse, GetByIdAttemptQuery> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var currentUser = _currentUserAccessor.User;
        if (currentUser == null)
            return Forbid();

        var query = new GetByIdAttemptQuery(id,  currentUser);

        var result = await handler.Handle(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "unauthorized" => Forbid(),
                "attempt.not_found" => NotFound(),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(result.Value);
    }

    [HttpPost("start")]
    [SwaggerOperation(
        OperationId = "StartAttempt",
        Summary = "Начать прохождение попытки",
        Description = "Создается попытка прохождения теста/экзамена и возвращает тест/экзамена для прохождения.")]
    public async Task<IActionResult> Start(
        [FromServices] ICommandHandler<StartResponse, StartAttemptCommand> handler,
        [FromBody] StartRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = _currentUserAccessor.User;
        if (currentUser == null)
            return Forbid();

        var command = new StartAttemptCommand(request, currentUser);

        var response = await handler.Handle(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("finish/{id:guid}")]
    [SwaggerOperation(
        OperationId = "Finish",
        Summary = "Закончить прохождение попытки",
        Description = "Возвращается попытка прохождения с результатами прохождения теста/экзамена.")]
    public async Task<IActionResult> Finish(
        [FromServices] ICommandHandler<AttemptResponse, FinishAttemptCommand> handler,
        [FromRoute] Guid id,
        [FromBody] FinishRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = _currentUserAccessor.User;
        if (currentUser == null)
            return Forbid();

        var command = new FinishAttemptCommand(id, request,  currentUser);

        var response = await handler.Handle(command, cancellationToken);
        return Ok(response);
    }
}