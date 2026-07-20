using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Questions.Tags.Features.CreateTagCommand;
using TestPlatform.Application.Questions.Tags.Features.DeleteTagCommand;
using TestPlatform.Application.Questions.Tags.Features.GetByIdTagQuery;
using TestPlatform.Application.Questions.Tags.Features.GetTagQuestionsQuery;
using TestPlatform.Application.Questions.Tags.Features.GetTagsQuery;
using TestPlatform.Application.Questions.Tags.Features.GetTagUsageQuery;
using TestPlatform.Application.Questions.Tags.Features.MergeTagsCommand;
using TestPlatform.Application.Questions.Tags.Features.UpdateTagCommand;
using TestPlatform.Contracts.Questions.DTOs.Preview;
using TestPlatform.Contracts.Tags.DTOs;
using TestPlatform.Presenters.Common.Validation;

namespace TestPlatform.Presenters.Tags;

[ApiController]
[Route("tags")]
public class TagsController : ControllerBase
{
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        OperationId = "GetByIdTag",
        Summary = "Получить тэг по Id.",
        Description = "Возвращает название тэга и ее описание по его Id")]
    public async Task<IActionResult> GetById(
        [FromServices] IQueryHandler<GetByIdTagQuery, TagResponse> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetByIdTagQuery(id);

        var result = await handler.Handle(query, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
    }

    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetTags",
        Summary = "Найти теги",
        Description = "Возвращает теги с поиском по названию и пагинацией.")]
    public async Task<IActionResult> GetAll(
        [FromServices] IQueryHandler<GetTagsQuery, TagPageResponse> handler,
        [FromQuery] string? search,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetTagsQuery(search, page, pageSize);

        var result = await handler.Handle(query, cancellationToken);

        return Ok(result.Value);
    }

    [HttpGet("suggestions")]
    [SwaggerOperation(
        OperationId = "GetTagSuggestions",
        Summary = "Получить подсказки тегов",
        Description = "Возвращает не более десяти тегов для автодополнения.")]
    public async Task<IActionResult> GetSuggestions(
        [FromServices] IQueryHandler<GetTagsQuery, TagPageResponse> handler,
        [FromQuery] string? query,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetTagsQuery(query, 1, 10), cancellationToken);
        return Ok(result.Value.Items);
    }

    [HttpGet("{id:guid}/usage")]
    [SwaggerOperation(
        OperationId = "GetTagUsage",
        Summary = "Получить использование тега",
        Description = "Возвращает количество вопросов, использующих тег.")]
    public async Task<IActionResult> GetUsage(
        [FromServices] IQueryHandler<GetTagUsageQuery, TagUsageResponse> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetTagUsageQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
    }

    [HttpGet("{id:guid}/questions")]
    [SwaggerOperation(
        OperationId = "GetTagQuestions",
        Summary = "Получить вопросы по тегу",
        Description = "Возвращает вопросы, использующие указанный тег, с пагинацией.")]
    public async Task<IActionResult> GetQuestions(
        [FromServices] IQueryHandler<GetTagQuestionsQuery, TagPageQuestionsResponse> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await handler.Handle(new GetTagQuestionsQuery(id, page, pageSize), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
    }

    [Authorize(Roles = "Admin,Teacher")]
    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreateTag",
        Summary = "Создать новый тэг",
        Description = "Создаёт новый тэг с указаным названием и опиманием.")]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<CreateTagCommand, Guid> handler,
        [FromServices] IValidator<TagRequest> validator,
        [FromBody] TagRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToValidationProblemDetails());
        }

        var command = new CreateTagCommand(request.Name, request.Description);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : result.Error == ErrorCodes.TagAlreadyExists
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        OperationId = "UpdateTag",
        Summary = "Обновить тэг",
        Description = "Обновить существующий тэг по Id с новыми данными: название, описание.")]
    public async Task<IActionResult> Update(
        [FromServices] ICommandHandler<UpdateTagCommand> handler,
        [FromServices] IValidator<TagRequest> validator,
        [FromRoute] Guid id,
        [FromBody] TagRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToValidationProblemDetails());
        }

        var command = new UpdateTagCommand(id, request.Name, request.Description);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess
            ? Ok()
            : result.Error == ErrorCodes.TagAlreadyExists
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{sourceId:guid}/merge/{targetId:guid}")]
    [SwaggerOperation(
        OperationId = "MergeTags",
        Summary = "Объединить теги",
        Description = "Переносит вопросы с исходного тега на целевой и удаляет исходный тег.")]
    public async Task<IActionResult> Merge(
        [FromServices] ICommandHandler<MergeTagsCommand, int> handler,
        [FromRoute] Guid sourceId,
        [FromRoute] Guid targetId,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new MergeTagsCommand(sourceId, targetId), cancellationToken);

        return result.IsSuccess
            ? Ok(new { affectedQuestionCount = result.Value })
            : result.Error == ErrorCodes.TagNotFound
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        OperationId = "DeleteTag",
        Summary = "Удалить тэг",
        Description = "Удаляет тэг по его индетификатору.")]
    public async Task<IActionResult> Delete(
        [FromServices] ICommandHandler<DeleteTagCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTagCommand(id);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error == ErrorCodes.TagInUse
                ? Conflict(new { error = result.Error })
                : NotFound(new { error = result.Error });
    }
}
