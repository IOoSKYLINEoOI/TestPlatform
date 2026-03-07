using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Questions.Features.CreateQuestionCommand;
using TestPlatform.Application.Questions.Features.DeleteQuestionCommand;
using TestPlatform.Application.Questions.Features.GetAllAllQuestionsByTagsQuery;
using TestPlatform.Application.Questions.Features.GetByIdQuestionQuery;
using TestPlatform.Application.Questions.Features.UpdateQuestionCommand;
using TestPlatform.Contracts.Questions.DTOs;

namespace TestPlatform.Presenters.Questions;

[ApiController]
[Route("[controller]")]
public class QuestionsController : ControllerBase
{
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        OperationId = "GetByIdQuestion",
        Summary = "Получить вопрос с ответами по Id.",
        Description = "Возвращает вопрос с ответами по его Id")]
    public async Task<IActionResult> GetById(
        [FromServices] IQueryHandler<QuestionResponse, GetByIdQuestionQuery> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken,
        [FromQuery] bool includeCorrectAnswer = false)
    {
        var query = new GetByIdQuestionQuery(id,  includeCorrectAnswer);

        var question = await handler.Handle(query, cancellationToken);
        return Ok(question);
    }

    [HttpGet("by-tags")]
    [SwaggerOperation(
        OperationId = "GetAllQuestionsByTags",
        Summary = "Получить все вопросы с определенными тэгами",
        Description = "Возвращает вопросы с ответами по определенным тэгам.")]
    public async Task<IActionResult> GetAll(
        [FromServices] IQueryHandler<List<QuestionResponse>, GetAllQuestionsByTagsQuery> handler,
        [FromQuery] List<Guid> tagIds,
        CancellationToken cancellationToken,
        [FromQuery] bool includeCorrectAnswer = false)
    {
        var query = new GetAllQuestionsByTagsQuery(tagIds, includeCorrectAnswer);

        var questions = await handler.Handle(query, cancellationToken);
        return Ok(questions);
    }

    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreateQuestion",
        Summary = "Создать новый вопрос с ответами",
        Description = "Создаёт новый вопрос с указаным текста, типа, стоимости, пути изображения, списка ответов (текст, корректность).")]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreateQuestionCommand> handler,
        [FromBody] QuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateQuestionCommand(request);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        OperationId = "UpdateQuestion",
        Summary = "Обновить вопрос с ответами",
        Description = "Обновить существующий вопрос по Id с новыми данными: текст, тип, стоимость, путь изображения, список ответов (текст, корректность).")]
    public async Task<IActionResult> Update(
        [FromServices] ICommandHandler<UpdateQuestionCommand> handler,
        [FromRoute] Guid id,
        [FromBody] UpdateQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateQuestionCommand(id, request);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

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
    }
}