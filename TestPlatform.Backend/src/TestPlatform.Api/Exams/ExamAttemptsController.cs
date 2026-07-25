using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TestPlatform.Api.Common;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Features.GetExamAttemptsQuery;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Attempts.Enums;
using TestPlatform.Contracts.Authorization;

namespace TestPlatform.Api.Exams;

[ApiController]
[Route("exams/{examId:guid}/attempts")]
[Authorize(Policy = AuthorizationPolicies.ManageContent)]
public sealed class ExamAttemptsController : ApiControllerBase
{
    [HttpGet]
    [SwaggerOperation(OperationId = "GetExamAttempts")]
    public async Task<IActionResult> GetAll(
        [FromServices] IQueryHandler<GetExamAttemptsQuery, ExamAttemptsPageResponse> handler,
        [FromRoute] Guid examId,
        [FromQuery] AttemptStatusDto? status = null,
        [FromQuery] bool? passed = null,
        [FromQuery] string? employeeNumber = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(
            new GetExamAttemptsQuery(examId, status, passed, employeeNumber, page, pageSize),
            cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : ToErrorResult(result.Error);
    }
}
