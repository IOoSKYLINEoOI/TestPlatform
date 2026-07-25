using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Questions.Features.CloneQuestionCommand;
using TestPlatform.Application.Questions.Features.ChangeQuestionStatusCommand;
using TestPlatform.Application.Questions.Features.CreateQuestionCommand;
using TestPlatform.Application.Questions.Features.GetQuestionsQuery;
using TestPlatform.Application.Questions.Features.GetByIdQuestionQuery;
using TestPlatform.Application.Questions.Features.UpdateQuestionCommand;
using TestPlatform.Contracts.Authorization;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Contracts.Questions.DTOs.Editor;
using TestPlatform.Core.Questions.Enums;
using TestPlatform.Api.Common;

namespace TestPlatform.Api.Questions;

[ApiController]
[Route("questions")]
public class QuestionsController : ApiControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        OperationId = "GetByIdQuestion",
        Summary = "Получить вопрос с ответами по Id.",
        Description = "Возвращает вопрос с ответами по его Id")]
    [ProducesResponseType(typeof(QuestionEditorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromServices] IQueryHandler<GetByIdQuestionQuery, QuestionEditorResponse> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetByIdQuestionQuery(id);

        var result = await handler.Handle(query, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : ToErrorResult(result.Error);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetQuestions",
        Summary = "Получить список вопросов",
        Description = "Возвращает превью вопросов с фильтрами по статусу и тегам, с пагинацией.")]
    [ProducesResponseType(typeof(QuestionPageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromServices] IQueryHandler<GetQuestionsQuery, QuestionPageResponse> handler,
        [FromQuery] List<Guid>? tagIds,
        [FromQuery] QuestionStatus? status,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetQuestionsQuery(tagIds ?? [], status, page, pageSize);

        var result = await handler.Handle(query, cancellationToken);
        return Ok(result.Value);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreateQuestion",
        Summary = "Создать новый вопрос с ответами(РАЗНЫЕ СХЕМЫ)",
        Description = "Создаёт новый вопрос и его варианты ответа.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<CreateQuestionCommand, Guid> handler,
        [FromBody] QuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateQuestionCommand(request);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : ToErrorResult(result.Error);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        OperationId = "UpdateQuestion",
        Summary = "Обновить вопрос с ответами(РАЗНЫЕ СХЕМЫ)",
        Description = "Обновить существующий вопрос по Id  и его варианты ответа.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromServices] ICommandHandler<UpdateQuestionCommand> handler,
        [FromRoute] Guid id,
        [FromBody] QuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateQuestionCommand(id, request);

        var result = await handler.Handle(command, cancellationToken);
        return ToCommandResult(result);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpPost("{id:guid}/publish")]
    [SwaggerOperation(
        OperationId = "PublishQuestion",
        Summary = "Опубликовать вопрос",
        Description = "Публикует черновик вопроса. Опубликованный вопрос больше нельзя изменять.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Publish(
        [FromServices] ICommandHandler<ChangeQuestionStatusCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ChangeQuestionStatusCommand(id, QuestionStatus.Published), cancellationToken);
        return ToCommandResult(result);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpPost("{id:guid}/archive")]
    [SwaggerOperation(
        OperationId = "ArchiveQuestion",
        Summary = "Архивировать вопрос",
        Description = "Архивирует вопрос и исключает его из дальнейшего использования.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(
        [FromServices] ICommandHandler<ChangeQuestionStatusCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ChangeQuestionStatusCommand(id, QuestionStatus.Archived), cancellationToken);
        return ToCommandResult(result);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpPost("{id:guid}/clone")]
    [SwaggerOperation(
        OperationId = "CloneQuestion",
        Summary = "Создать черновик-копию вопроса",
        Description = "Создаёт независимый черновик из опубликованного вопроса для последующего редактирования.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Clone(
        [FromServices] ICommandHandler<CloneQuestionCommand, Guid> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new CloneQuestionCommand(id), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : ToErrorResult(result.Error);
    }

    private IActionResult ToCommandResult(CSharpFunctionalExtensions.Result result)
    {
        return result.IsSuccess
            ? NoContent()
            : ToErrorResult(result.Error);
    }
}
