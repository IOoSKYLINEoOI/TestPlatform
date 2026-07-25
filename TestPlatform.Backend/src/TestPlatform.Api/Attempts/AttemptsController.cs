using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Features.AbandonAttemptCommand;
using TestPlatform.Application.Attempts.Features.CancelAttemptCommand;
using TestPlatform.Application.Attempts.Features.FinishAttemptCommand;
using TestPlatform.Application.Attempts.Features.GetByIdAttemptDetailsQuery;
using TestPlatform.Application.Attempts.Features.GetByIdAttemptQuery;
using TestPlatform.Application.Attempts.Features.GetMyAttemptsQuery;
using TestPlatform.Application.Attempts.Features.RemoveAttemptAnswerCommand;
using TestPlatform.Application.Attempts.Features.SaveAttemptAnswerCommand;
using TestPlatform.Application.Attempts.Features.StartAttemptCommand;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Request;
using TestPlatform.Contracts.Attempts.Enums;
using TestPlatform.Contracts.Authorization;
using TestPlatform.Api.Common;

namespace TestPlatform.Api.Attempts;

[ApiController]
[Authorize]
[Route("attempts")]
public class AttemptsController : ApiControllerBase
{
    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetMyAttempts",
        Summary = "Получить историю своих попыток")]
    public async Task<IActionResult> GetMyAttempts(
        [FromServices] IQueryHandler<GetMyAttemptsQuery, AttemptHistoryPageResponse> handler,
        [FromQuery] AttemptTypeDto? type = null,
        [FromQuery] AttemptStatusDto? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(
            new GetMyAttemptsQuery(type, status, page, pageSize),
            cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToErrorResult(result.Error);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        OperationId = "GetByIdAttempt",
        Summary = "Получить состояние попытки")]
    public async Task<IActionResult> GetById(
        [FromServices] IQueryHandler<GetByIdAttemptQuery, AttemptResponse> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetByIdAttemptQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToErrorResult(result.Error);
    }

    [HttpPost]
    [SwaggerOperation(
        OperationId = "StartAttempt",
        Summary = "Начать попытку")]
    public async Task<IActionResult> Start(
        [FromServices] ICommandHandler<StartAttemptCommand, StartAttemptResponse> handler,
        [FromBody] StartRequest request,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new StartAttemptCommand(request), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.AttemptId }, result.Value)
            : ToErrorResult(result.Error);
    }

    [HttpPut("{id:guid}/answers")]
    [SwaggerOperation(
        OperationId = "SaveAttemptAnswer",
        Summary = "Сохранить ответ на вопрос попытки")]
    public async Task<IActionResult> SaveAnswer(
        [FromServices] ICommandHandler<SaveAttemptAnswerCommand> handler,
        [FromRoute] Guid id,
        [FromBody] AttemptAnswerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new SaveAttemptAnswerCommand(id, request),
            cancellationToken);
        return result.IsSuccess ? NoContent() : ToErrorResult(result.Error);
    }

    [HttpDelete("{id:guid}/answers/{questionId:guid}")]
    public async Task<IActionResult> RemoveAnswer(
        [FromServices] ICommandHandler<RemoveAttemptAnswerCommand> handler,
        [FromRoute] Guid id,
        [FromRoute] Guid questionId,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new RemoveAttemptAnswerCommand(id, questionId),
            cancellationToken);
        return result.IsSuccess ? NoContent() : ToErrorResult(result.Error);
    }

    [HttpPost("{id:guid}/finish")]
    [SwaggerOperation(
        OperationId = "FinishAttempt",
        Summary = "Завершить попытку")]
    public async Task<ActionResult<AttemptResultResponse>> Finish(
        [FromServices] ICommandHandler<FinishAttemptCommand, AttemptResultResponse> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new FinishAttemptCommand(id), cancellationToken);
        return result.IsSuccess
            ? result.Value
            : new ActionResult<AttemptResultResponse>((ActionResult)ToErrorResult(result.Error));
    }

    [HttpPost("{id:guid}/abandon")]
    [SwaggerOperation(
        OperationId = "AbandonAttempt",
        Summary = "Прервать попытку без результата")]
    public async Task<IActionResult> Abandon(
        [FromServices] ICommandHandler<AbandonAttemptCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new AbandonAttemptCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : ToErrorResult(result.Error);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageSystem)]
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        [FromServices] ICommandHandler<CancelAttemptCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new CancelAttemptCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : ToErrorResult(result.Error);
    }

    [HttpGet("{id:guid}/result")]
    [SwaggerOperation(
        OperationId = "GetAttemptResult",
        Summary = "Получить детальный результат завершённой попытки")]
    public async Task<ActionResult<AttemptDetailsResponse>> GetResult(
        [FromServices] IQueryHandler<GetByIdAttemptDetailsQuery, AttemptDetailsResponse> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetByIdAttemptDetailsQuery(id), cancellationToken);
        return result.IsSuccess
            ? result.Value
            : new ActionResult<AttemptDetailsResponse>((ActionResult)ToErrorResult(result.Error));
    }

}
