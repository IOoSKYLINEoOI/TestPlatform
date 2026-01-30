using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Categories.Features.CreateCategory;
using TestPlatform.Application.Categories.Features.DeleteCategory;
using TestPlatform.Application.Categories.Features.UpdateCategory;
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

    /*[HttpGet("{id:int}")]
    [SwaggerOperation(
        OperationId = "GetCategory",
        Summary = "Получить категорию по Id.",
        Description = "Возвращает название категории и ее описание по ее Id")]
    public async Task<IActionResult> GetCategory([FromRoute] int id)
    {
        var categoryResult = await _categoryService.GetCategoryById(id);
        if (categoryResult.IsFailure)
            return NotFound(categoryResult.Error);

        var response = new CategoryResponse(
            categoryResult.Value.Id,
            categoryResult.Value.Name, 
            categoryResult.Value.Description);

        return Ok(response);
    }*/

    /*[HttpGet("all")]
    [SwaggerOperation(
        OperationId = "GetAllCategories",
        Summary = "Получить все категории",
        Description = "Возвращает название и описание все категорий.")]
    public async Task<IActionResult> GetAllCategories()
    {
        var categories = await _categoryService.GetAllCategories();
        var response = categories.Select(t => new CategoryResponse(
            t.Id, 
            t.Name, 
            t.Description
        )).ToList();

        return Ok(response);
    }*/

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