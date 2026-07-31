using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Api.Common;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Exams.Features.ArchiveExamCommand;
using TestPlatform.Application.Exams.Features.CreateExamCommand;
using TestPlatform.Application.Exams.Features.GetByIdExamQuery;
using TestPlatform.Application.Exams.Features.GetExamCatalogQuery;
using TestPlatform.Application.Exams.Features.GetExamManagementQuery;
using TestPlatform.Application.Exams.Features.PublishExamCommand;
using TestPlatform.Application.Exams.Features.UpdateExamDetailsCommand;
using TestPlatform.Contracts.Authorization;
using TestPlatform.Contracts.Exams.DTOs;

namespace TestPlatform.Api.Exams;

[ApiController]
[Route("exams")]
public sealed class ExamsController : ApiControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpGet("management")]
    [SwaggerOperation(OperationId = "GetExamManagementList")]
    public async Task<IActionResult> GetManagementList(
        [FromServices] IQueryHandler<GetExamManagementQuery, ExamManagementPageResponse> handler,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(
            new GetExamManagementQuery(search, page, pageSize),
            cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToErrorResult(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetCatalog(
        [FromServices] IQueryHandler<GetExamCatalogQuery, ExamCatalogPageResponse> handler,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(new GetExamCatalogQuery(page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToErrorResult(result.Error);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpGet("{id:guid}")]
    [SwaggerOperation(OperationId = "GetByIdExam")]
    public async Task<IActionResult> GetById(
        [FromServices] IQueryHandler<GetByIdExamQuery, ExamFullResponse> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetByIdExamQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToErrorResult(result.Error);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpPost]
    [SwaggerOperation(OperationId = "CreateExam")]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<CreateExamCommand, Guid> handler,
        [FromBody] ExamRequest request,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new CreateExamCommand(request), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : ToErrorResult(result.Error);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpPatch("{id:guid}")]
    [SwaggerOperation(OperationId = "UpdateExamDetails")]
    public async Task<IActionResult> UpdateDetails(
        [FromServices] ICommandHandler<UpdateExamDetailsCommand> handler,
        [FromRoute] Guid id,
        [FromBody] UpdateExamDetailsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new UpdateExamDetailsCommand(id, request), cancellationToken);
        return result.IsSuccess ? NoContent() : ToErrorResult(result.Error);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpPost("{id:guid}/publish")]
    [SwaggerOperation(OperationId = "PublishExam")]
    public async Task<IActionResult> Publish(
        [FromServices] ICommandHandler<PublishExamCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new PublishExamCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : ToErrorResult(result.Error);
    }

    [Authorize(Policy = AuthorizationPolicies.ManageContent)]
    [HttpPost("{id:guid}/archive")]
    [SwaggerOperation(OperationId = "ArchiveExam")]
    public async Task<IActionResult> Archive(
        [FromServices] ICommandHandler<ArchiveExamCommand> handler,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ArchiveExamCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : ToErrorResult(result.Error);
    }
}
