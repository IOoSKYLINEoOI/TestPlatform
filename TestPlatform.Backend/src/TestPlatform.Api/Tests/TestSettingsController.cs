using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestPlatform.Api.Common;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Tests.Features.DeleteTestCoverImageCommand;
using TestPlatform.Application.Tests.Features.DeleteTestTimeLimitCommand;
using TestPlatform.Application.Tests.Features.UpdateTestCoverImageCommand;
using TestPlatform.Application.Tests.Features.UpdateTestTimeLimitCommand;
using TestPlatform.Contracts.Authorization;
using TestPlatform.Contracts.Common;

namespace TestPlatform.Api.Tests;

[ApiController]
[Route("tests/{testId:guid}")]
[Authorize(Policy = AuthorizationPolicies.ManageContent)]
public sealed class TestSettingsController : ApiControllerBase
{
    [HttpPut("time-limit")]
    public Task<IActionResult> UpdateTimeLimit(
        [FromServices] ICommandHandler<UpdateTestTimeLimitCommand> handler,
        [FromRoute] Guid testId,
        [FromBody] UpdateTimeLimitRequest request,
        CancellationToken cancellationToken) =>
        Handle(handler, new UpdateTestTimeLimitCommand(testId, request), cancellationToken);

    [HttpDelete("time-limit")]
    public Task<IActionResult> DeleteTimeLimit(
        [FromServices] ICommandHandler<DeleteTestTimeLimitCommand> handler,
        [FromRoute] Guid testId,
        CancellationToken cancellationToken) =>
        Handle(handler, new DeleteTestTimeLimitCommand(testId), cancellationToken);

    [HttpPut("cover-image")]
    public Task<IActionResult> UpdateCoverImage(
        [FromServices] ICommandHandler<UpdateTestCoverImageCommand> handler,
        [FromRoute] Guid testId,
        [FromBody] UpdateCoverImageRequest request,
        CancellationToken cancellationToken) =>
        Handle(handler, new UpdateTestCoverImageCommand(testId, request), cancellationToken);

    [HttpDelete("cover-image")]
    public Task<IActionResult> DeleteCoverImage(
        [FromServices] ICommandHandler<DeleteTestCoverImageCommand> handler,
        [FromRoute] Guid testId,
        CancellationToken cancellationToken) =>
        Handle(handler, new DeleteTestCoverImageCommand(testId), cancellationToken);

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
