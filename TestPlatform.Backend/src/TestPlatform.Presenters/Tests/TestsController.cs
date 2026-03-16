using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Tests.Features.CreateTestCommand;
using TestPlatform.Application.Tests.Features.DeleteTestCommand;
using TestPlatform.Application.Tests.Features.GetAllTestsQuery;
using TestPlatform.Application.Tests.Features.GetByIdTestQuery;
using TestPlatform.Application.Tests.Features.UpdateTestCommand;
using TestPlatform.Contracts.Tests.DTOs;

namespace TestPlatform.Presenters.Tests;

[ApiController]
[Route("tests")]
public class TestsController : ControllerBase
{
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        OperationId = "GetByIdTest",
        Summary = "Получить тест по Id.",
        Description = "Возвращает тест с названием, ограничением времени, описанием, ид автора, количеством вопросов и тэгами по его Id")]
    public async Task<IActionResult> GetById(
        [FromServices] IQueryHandler<TestResponse, GetByIdTestQuery> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken,
        [FromQuery] bool includeCorrectAnswer = false)
    {
        var query = new GetByIdTestQuery(id, includeCorrectAnswer);

        var test = await handler.Handle(query, cancellationToken);
        return Ok(test);
    }

    [HttpGet("all")]
    [SwaggerOperation(
        OperationId = "GetAllTests",
        Summary = "Получить все тесты",
        Description = "Возвращает название, ограничение времени, описание, автора, количество вопросов и список тэгов всех тестов.")]
    public async Task<IActionResult> GetAll(
        [FromServices] IQueryHandler<List<TestResponse>, GetAllTestsQuery> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetAllTestsQuery();

        var tests = await handler.Handle(query, cancellationToken);
        return Ok(tests);
    }

    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreateTest",
        Summary = "Создать новый тест",
        Description = "Создаёт новый тест с указаным названием и описанием, ограничением по времени, кавером, ид автора, и списком вопросов.")]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<Guid, CreateTestCommand> handler,
        [FromBody] TestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTestCommand(request);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        OperationId = "UpdateTest",
        Summary = "Обновить тест",
        Description = "Обновить существующий тест по Id с новыми данными: название, описание, ограничением по времени, кавером, ид автора, и списком вопросов.")]
    public async Task<IActionResult> Update(
        [FromServices] ICommandHandler<UpdateTestCommand> handler,
        [FromRoute] Guid id,
        [FromBody] TestRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTestCommand(id, request);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        OperationId = "DeleteTest",
        Summary = "Удалить тест",
        Description = "Удаляет тест по его индетификатору.")]
    public async Task<IActionResult> Delete(
        [FromServices] ICommandHandler<DeleteTestCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTestCommand(id);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}