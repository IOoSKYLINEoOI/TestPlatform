using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Tests.Features.CreateTestCommand;
using TestPlatform.Application.Tests.Features.DeleteTestCommand;
using TestPlatform.Application.Tests.Features.GetAllTestsQuery;
using TestPlatform.Application.Tests.Features.GetByIdTestQuery;
using TestPlatform.Application.Tests.Features.UpdateTestCommand;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Tests.DTOs;
using TestPlatform.Contracts.Users.DTOs;

namespace TestPlatform.Presenters.Tests;

[ApiController]
[Route("tests")]
public class TestsController : ControllerBase
{
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public TestsController(ICurrentUserAccessor currentUserAccessor)
    {
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        OperationId = "GetByIdTest",
        Summary = "Получить тест по Id.",
        Description = "Возвращает тест с названием, ограничением времени, описанием, ид автора, количеством вопросов и тэгами по его Id")]
    public async Task<IActionResult> GetById(
        [FromServices] IQueryHandler<TestFullResponse, GetByIdTestQuery> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken,
        [FromQuery] bool includeCorrectAnswer = false)
    {
        var query = new GetByIdTestQuery(id, includeCorrectAnswer);

        var result = await handler.Handle(query, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
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

        var result = await handler.Handle(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
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
        var currentUser = _currentUserAccessor.User;
        if (currentUser == null)
            return Forbid();

        var command = new CreateTestCommand(request, currentUser);

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
        var currentUser = _currentUserAccessor.User;
        if (currentUser == null)
            return Forbid();

        var command = new UpdateTestCommand(id, request, currentUser);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? Ok() : result.Error == "Forbidden" ? Forbid() : BadRequest(result.Error);
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
        var currentUser = HttpContext.Items["CurrentUser"] as CurrentUserDto;
        if (currentUser == null)
            return Forbid();

        var command = new DeleteTestCommand(id, currentUser);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? NoContent() : result.Error == "Forbidden" ? Forbid() : NotFound(result.Error);
    }
}