using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Questions.Features.CreateQuestionCommand;
using TestPlatform.Application.Questions.Features.GetAllQuestionsByTagsQuery;
using TestPlatform.Application.Questions.Features.GetByIdQuestionQuery;
using TestPlatform.Application.Questions.Features.UpdateQuestionCommand;
using TestPlatform.Contracts.Questions.DTOs;

namespace TestPlatform.Presenters.Questions;

[ApiController]
[Route("questions")]
public class QuestionsController : ControllerBase
{
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        OperationId = "GetByIdQuestion",
        Summary = "Получить вопрос с ответами по Id.",
        Description = "Возвращает вопрос с ответами по его Id")]
    [ProducesResponseType(typeof(QuestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromServices] IQueryHandler<GetByIdQuestionQuery, QuestionResponse> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetByIdQuestionQuery(id);

        var result = await handler.Handle(query, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
    }

    [HttpGet("by-tags")]
    [SwaggerOperation(
        OperationId = "GetAllQuestionsByTags",
        Summary = "Получить все вопросы с определенными тэгами",
        Description = "Возвращает вопросы с ответами по определенным тэгам.")]
    [ProducesResponseType(typeof(IReadOnlyList<QuestionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(
        [FromServices] IQueryHandler<GetAllQuestionsByTagsQuery, IReadOnlyList<QuestionResponse>> handler,
        [FromQuery] List<Guid> tagIds,
        CancellationToken cancellationToken)
    {
        var query = new GetAllQuestionsByTagsQuery(tagIds);

        var result = await handler.Handle(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreateQuestion",
        Summary = "Создать новый вопрос с ответами(РАЗНЫЕ СХЕМЫ)",
        Description = "Создаёт новый вопрос и его варианты ответа.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<CreateQuestionCommand, Guid> handler,
        [FromBody] QuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateQuestionCommand(request);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [Authorize(Roles = "Teacher,Admin")]
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        OperationId = "UpdateQuestion",
        Summary = "Обновить вопрос с ответами(РАЗНЫЕ СХЕМЫ)",
        Description = "Обновить существующий вопрос по Id  и его варианты ответа.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        [FromServices] ICommandHandler<UpdateQuestionCommand> handler,
        [FromRoute] Guid id,
        [FromBody] QuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateQuestionCommand(id, request);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    /*Authorize(Roles = "Teacher,Admin")]
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        OperationId = "DeleteQuestion",
        Summary = "Удалить вопрос с ответами",
        Description = "Удаляет вопрос с ответами по его индетификатору.")]
    public async Task<IActionResult> Delete(
        [FromServices] ICommandHandler<DeleteQuestionCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteQuestionCommand(id);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }*/
}
