using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Api.Common;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Exams.Features.ManageExamSections;
using TestPlatform.Contracts.Authorization;
using TestPlatform.Contracts.Exams.DTOs;

namespace TestPlatform.Api.Exams;

[ApiController]
[Route("exams/{examId:guid}/sections")]
[Authorize(Policy = AuthorizationPolicies.ManageContent)]
public sealed class ExamSectionsController : ApiControllerBase
{
    [HttpPost]
    [SwaggerOperation(OperationId = "AddExamSection")]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<AddExamSectionCommand, Guid> handler,
        [FromRoute] Guid examId,
        [FromBody] CreateExamSectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new AddExamSectionCommand(examId, request), cancellationToken);
        return result.IsSuccess
            ? Created($"/exams/{examId}", new { id = result.Value })
            : ToErrorResult(result.Error);
    }

    [HttpPatch("{sectionId:guid}")]
    public async Task<IActionResult> Update(
        [FromServices] ICommandHandler<UpdateExamSectionCommand> handler,
        [FromRoute] Guid examId,
        [FromRoute] Guid sectionId,
        [FromBody] UpdateExamSectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new UpdateExamSectionCommand(examId, sectionId, request),
            cancellationToken);
        return result.IsSuccess ? NoContent() : ToErrorResult(result.Error);
    }

    [HttpDelete("{sectionId:guid}")]
    public async Task<IActionResult> Delete(
        [FromServices] ICommandHandler<RemoveExamSectionCommand> handler,
        [FromRoute] Guid examId,
        [FromRoute] Guid sectionId,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new RemoveExamSectionCommand(examId, sectionId),
            cancellationToken);
        return result.IsSuccess ? NoContent() : ToErrorResult(result.Error);
    }

    [HttpPost("{sectionId:guid}/questions")]
    public async Task<IActionResult> AddQuestion(
        [FromServices] ICommandHandler<AddQuestionToExamSectionCommand> handler,
        [FromRoute] Guid examId,
        [FromRoute] Guid sectionId,
        [FromBody] AddExamSectionQuestionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new AddQuestionToExamSectionCommand(examId, sectionId, request.QuestionId),
            cancellationToken);
        return result.IsSuccess ? NoContent() : ToErrorResult(result.Error);
    }

    [HttpDelete("{sectionId:guid}/questions/{questionId:guid}")]
    public async Task<IActionResult> RemoveQuestion(
        [FromServices] ICommandHandler<RemoveQuestionFromExamSectionCommand> handler,
        [FromRoute] Guid examId,
        [FromRoute] Guid sectionId,
        [FromRoute] Guid questionId,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(
            new RemoveQuestionFromExamSectionCommand(examId, sectionId, questionId),
            cancellationToken);
        return result.IsSuccess ? NoContent() : ToErrorResult(result.Error);
    }
}
