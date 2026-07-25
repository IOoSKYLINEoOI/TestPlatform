using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Api.Common;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Features.GetTestAttemptsQuery;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Attempts.Enums;
using TestPlatform.Contracts.Authorization;

namespace TestPlatform.Api.Tests;

[ApiController]
[Route("tests/{testId:guid}/attempts")]
[Authorize(Policy = AuthorizationPolicies.ManageContent)]
public sealed class TestAttemptsController : ApiControllerBase
{
    [HttpGet]
    [SwaggerOperation(OperationId = "GetTestAttempts")]
    [ProducesResponseType(typeof(TestAttemptsPageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromServices] IQueryHandler<GetTestAttemptsQuery, TestAttemptsPageResponse> handler,
        [FromRoute] Guid testId,
        [FromQuery] AttemptStatusDto? status = null,
        [FromQuery] string? employeeNumber = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(
            new GetTestAttemptsQuery(testId, status, employeeNumber, page, pageSize),
            cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToErrorResult(result.Error);
    }
}
