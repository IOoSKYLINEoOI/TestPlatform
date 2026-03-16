using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Tags.Features.CreateTagCommand;
using TestPlatform.Application.Tags.Features.DeleteTagCommand;
using TestPlatform.Application.Tags.Features.GetAllTagsQuery;
using TestPlatform.Application.Tags.Features.GetByIdTagQuery;
using TestPlatform.Application.Tags.Features.UpdateTagCommand;
using TestPlatform.Contracts.Tags.DTOs;

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
        [FromServices] IQueryHandler<TagResponse, GetByIdTagQuery> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetByIdTagQuery(id);

        var tag = await handler.Handle(query, cancellationToken);
        return Ok(tag);
    }

    [HttpGet("all")]
    [SwaggerOperation(
        OperationId = "GetAllTags",
        Summary = "Получить все тэги",
        Description = "Возвращает название и описание всех тэгов.")]
    public async Task<IActionResult> GetAll(
        [FromServices] IQueryHandler<List<TagResponse>, GetAllTagsQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetAllTagsQuery();

        var tags = await handler.Handle(query, cancellationToken);
        return Ok(tags);
    }

    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreateTag",
        Summary = "Создать новый тэг",
        Description = "Создаёт новый тэг с указаным названием и опиманием.")]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreateTagCommand> handler,
        [FromBody] TagRequest request,
        CancellationToken cancellationToken)
    {
       var command = new CreateTagCommand(request.Name, request.Description);

       var result = await handler.Handle(command, cancellationToken);
       return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        OperationId = "UpdateTag",
        Summary = "Обновить тэг",
        Description = "Обновить существующий тэг по Id с новыми данными: название, описание.")]
    public async Task<IActionResult> Update(
        [FromServices] ICommandHandler<UpdateTagCommand> handler,
        [FromRoute] Guid id,
        [FromBody] TagRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTagCommand(id, request.Name, request.Description);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

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
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}