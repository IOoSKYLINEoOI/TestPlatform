using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Api.Common;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Exams.Features.DeleteExamCoverImageCommand;
using TestPlatform.Application.Exams.Features.DeleteExamScheduleCommand;
using TestPlatform.Application.Exams.Features.DeleteExamTimeLimitCommand;
using TestPlatform.Application.Exams.Features.ManageExamSections;
using TestPlatform.Application.Exams.Features.UpdateExamCoverImageCommand;
using TestPlatform.Application.Exams.Features.UpdateExamPassingRuleCommand;
using TestPlatform.Application.Exams.Features.UpdateExamScheduleCommand;
using TestPlatform.Application.Exams.Features.UpdateExamTimeLimitCommand;
using TestPlatform.Contracts.Authorization;
using TestPlatform.Contracts.Common;
using TestPlatform.Contracts.Exams.DTOs;

namespace TestPlatform.Api.Exams;

[ApiController]
[Route("exams/{examId:guid}")]
[Authorize(Policy = AuthorizationPolicies.ManageContent)]
public sealed class ExamSettingsController : ApiControllerBase
{
    [HttpPut("time-limit")]
    [SwaggerOperation(OperationId = "UpdateExamTimeLimit")]
    public Task<IActionResult> UpdateTimeLimit(
        [FromServices] ICommandHandler<UpdateExamTimeLimitCommand> handler,
        [FromRoute] Guid examId,
        [FromBody] UpdateTimeLimitRequest request,
        CancellationToken cancellationToken) =>
        Handle(handler, new UpdateExamTimeLimitCommand(examId, request), cancellationToken);

    [HttpDelete("time-limit")]
    [SwaggerOperation(OperationId = "DeleteExamTimeLimit")]
    public Task<IActionResult> DeleteTimeLimit(
        [FromServices] ICommandHandler<DeleteExamTimeLimitCommand> handler,
        [FromRoute] Guid examId,
        CancellationToken cancellationToken) =>
        Handle(handler, new DeleteExamTimeLimitCommand(examId), cancellationToken);

    [HttpPut("cover-image")]
    [SwaggerOperation(OperationId = "UpdateExamCoverImage")]
    public Task<IActionResult> UpdateCoverImage(
        [FromServices] ICommandHandler<UpdateExamCoverImageCommand> handler,
        [FromRoute] Guid examId,
        [FromBody] UpdateCoverImageRequest request,
        CancellationToken cancellationToken) =>
        Handle(handler, new UpdateExamCoverImageCommand(examId, request), cancellationToken);

    [HttpDelete("cover-image")]
    [SwaggerOperation(OperationId = "DeleteExamCoverImage")]
    public Task<IActionResult> DeleteCoverImage(
        [FromServices] ICommandHandler<DeleteExamCoverImageCommand> handler,
        [FromRoute] Guid examId,
        CancellationToken cancellationToken) =>
        Handle(handler, new DeleteExamCoverImageCommand(examId), cancellationToken);

    [HttpPut("schedule")]
    [SwaggerOperation(OperationId = "UpdateExamSchedule")]
    public Task<IActionResult> UpdateSchedule(
        [FromServices] ICommandHandler<UpdateExamScheduleCommand> handler,
        [FromRoute] Guid examId,
        [FromBody] UpdateExamScheduleRequest request,
        CancellationToken cancellationToken) =>
        Handle(handler, new UpdateExamScheduleCommand(examId, request), cancellationToken);

    [HttpDelete("schedule")]
    [SwaggerOperation(OperationId = "DeleteExamSchedule")]
    public Task<IActionResult> DeleteSchedule(
        [FromServices] ICommandHandler<DeleteExamScheduleCommand> handler,
        [FromRoute] Guid examId,
        CancellationToken cancellationToken) =>
        Handle(handler, new DeleteExamScheduleCommand(examId), cancellationToken);

    [HttpPut("passing-rule")]
    [SwaggerOperation(OperationId = "UpdateExamPassingRule")]
    public Task<IActionResult> UpdatePassingRule(
        [FromServices] ICommandHandler<UpdateExamPassingRuleCommand> handler,
        [FromRoute] Guid examId,
        [FromBody] UpdateExamPassingRuleRequest request,
        CancellationToken cancellationToken) =>
        Handle(handler, new UpdateExamPassingRuleCommand(examId, request), cancellationToken);

    [HttpPut("attempts-limit")]
    public Task<IActionResult> UpdateAttemptsLimit(
        [FromServices] ICommandHandler<UpdateExamAttemptsLimitCommand> handler,
        [FromRoute] Guid examId,
        [FromBody] UpdateExamAttemptsLimitRequest request,
        CancellationToken cancellationToken) =>
        Handle(handler, new UpdateExamAttemptsLimitCommand(examId, request.AttemptsLimit), cancellationToken);

    [HttpPut("review-policy")]
    public Task<IActionResult> UpdateReviewPolicy(
        [FromServices] ICommandHandler<UpdateExamReviewPolicyCommand> handler,
        [FromRoute] Guid examId,
        [FromBody] UpdateExamReviewPolicyRequest request,
        CancellationToken cancellationToken) =>
        Handle(handler, new UpdateExamReviewPolicyCommand(examId, request.ReviewPolicy), cancellationToken);

    private async Task<IActionResult> Handle<TCommand>(
        ICommandHandler<TCommand> handler,
        TCommand command,
        CancellationToken cancellationToken)
        where TCommand : ICommand
    {
        var result = await handler.Handle(command, cancellationToken);
        return result.IsSuccess ? NoContent() : ToErrorResult(result.Error);
    }
}
