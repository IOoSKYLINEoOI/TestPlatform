using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Categories.Features.CreateCategoryCommand;
using TestPlatform.Application.Categories.Features.DeleteCategoryCommand;
using TestPlatform.Application.Categories.Features.GetAllQuery;
using TestPlatform.Application.Categories.Features.GetByIdCategoryQuery;
using TestPlatform.Application.Categories.Features.UpdateCategoryCommand;
using TestPlatform.Contracts.Categories.DTOs;

namespace TestPlatform.Presenters.Categories;

[ApiController]
[Route("[controller]")]
public class CategoriesController : ControllerBase
{
    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreateCategory",
        Summary = "Создать новую категорию",
        Description = "Создаёт новую категорию с указаным названием и опиманием.")]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreateCategoryCommand> handler,
        [FromBody] CategoryRequest request,
        CancellationToken cancellationToken)
    {
       var command = new CreateCategoryCommand(request.Name, request.Description);

       var result = await handler.Handle(command, cancellationToken);
       return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        OperationId = "UpdateCategory",
        Summary = "Обновить категорию",
        Description = "Обновить существующую категорию по Id с новыми данными: название, описание.")]
    public async Task<IActionResult> Update(
        [FromServices] ICommandHandler<UpdateCategoryCommand> handler,
        [FromRoute] Guid id,
        [FromBody] CategoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand(id, request.Name, request.Description);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        OperationId = "GetByIdCategory",
        Summary = "Получить категорию по Id.",
        Description = "Возвращает название категории и ее описание по ее Id")]
    public async Task<IActionResult> GetById(
        [FromServices] IQueryHandler<CategoryResponse, GetByIdCategoryQuery> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetByIdCategoryQuery(id);

        var category = await handler.Handle(query, cancellationToken);
        return category is not null ? Ok(category) : NotFound();
    }

    [HttpGet("all")]
    [SwaggerOperation(
        OperationId = "GetAllCategories",
        Summary = "Получить все категории",
        Description = "Возвращает название и описание все категорий.")]
    public async Task<IActionResult> GetAll(
        [FromServices] IQueryHandler<List<CategoryResponse>, GetAllQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetAllQuery();

        var categories = await handler.Handle(query, cancellationToken);
        return Ok(categories);
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        OperationId = "DeleteCategory",
        Summary = "Удалить категорию",
        Description = "Удаляет категорию по ее индетификатору.")]
    public async Task<IActionResult> Delete(
        [FromServices] ICommandHandler<DeleteCategoryCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand(id);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}