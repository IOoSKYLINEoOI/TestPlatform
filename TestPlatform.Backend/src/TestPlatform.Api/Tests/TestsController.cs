using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Tests.Features.AddTestQuestionCommand;
using TestPlatform.Application.Tests.Features.CreateTestCommand;
using TestPlatform.Application.Tests.Features.DeleteTestQuestionCommand;
using TestPlatform.Application.Tests.Features.ArchiveTestCommand;
using TestPlatform.Application.Tests.Features.GetAllTestsQuery;
using TestPlatform.Application.Tests.Features.GetByIdTestQuery;
using TestPlatform.Application.Tests.Features.PublishTestCommand;
using TestPlatform.Application.Users;
using Microsoft.AspNetCore.Authorization;
using TestPlatform.Contracts.Authorization;
using TestPlatform.Api.Common;
using TestPlatform.Contracts.Tests.DTOs;

namespace TestPlatform.Api.Tests;

[ApiController]
[Route("tests")]
public class TestsController : ApiControllerBase
{
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public TestsController(ICurrentUserAccessor currentUserAccessor)
    {
        _currentUserAccessor = currentUserAccessor;
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        OperationId = "GetByIdTest",
        Summary = "Получить тест по Id.",
        Description = "Возвращает тест с названием, ограничением времени, описанием, ид автора, количеством вопросов и тэгами по его Id")]
    public async Task<IActionResult> GetById(
        [FromServices] IQueryHandler<GetByIdTestQuery, TestFullResponse> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetByIdTestQuery(id);

        var result = await handler.Handle(query, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : ToErrorResult(result.Error);
    }

    [HttpGet]
    [SwaggerOperation(
        OperationId = "GetAllTests",
        Summary = "Получить все тесты",
        Description = "Возвращает название, ограничение времени, описание, автора, количество вопросов и список тэгов всех тестов.")]
    public async Task<IActionResult> GetAll(
        [FromServices] IQueryHandler<GetAllTestsQuery, IReadOnlyList<TestResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetAllTestsQuery();

        var result = await handler.Handle(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToErrorResult(result.Error);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpPost]
    [SwaggerOperation(
        OperationId = "CreateTest",
        Summary = "Создать новый тест",
        Description = "Создаёт новый тест с указаным названием и описанием, ограничением по времени, кавером, ид автора, и списком вопросов.")]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<CreateTestCommand, Guid> handler,
        [FromBody] TestRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = _currentUserAccessor.User;
        if (currentUser == null)
        {
            return Unauthorized();
        }

        var command = new CreateTestCommand(request);

        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : ToErrorResult(result.Error);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(
        [FromServices] ICommandHandler<PublishTestCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new PublishTestCommand(id), cancellationToken);
        return ToCommandResult(result);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpPost("{id:guid}/archive")]
    public async Task<IActionResult> Archive(
        [FromServices] ICommandHandler<ArchiveTestCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ArchiveTestCommand(id), cancellationToken);
        return ToCommandResult(result);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpPost("{id:guid}/questions")]
    public async Task<IActionResult> AddQuestion(
        [FromServices] ICommandHandler<AddTestQuestionCommand> handler,
        [FromRoute] Guid id,
        [FromBody] AddTestQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new AddTestQuestionCommand(id, request.QuestionId),
            cancellationToken);
        return ToCommandResult(result);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpDelete("{id:guid}/questions/{questionId:guid}")]
    public async Task<IActionResult> DeleteQuestion(
        [FromServices] ICommandHandler<DeleteTestQuestionCommand> handler,
        [FromRoute] Guid id,
        [FromRoute] Guid questionId,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new DeleteTestQuestionCommand(id, questionId),
            cancellationToken);
        return ToCommandResult(result);
    }

    private IActionResult ToCommandResult(CSharpFunctionalExtensions.Result result)
    {
        return result.IsSuccess
            ? NoContent()
            : ToErrorResult(result.Error);
    }

}
