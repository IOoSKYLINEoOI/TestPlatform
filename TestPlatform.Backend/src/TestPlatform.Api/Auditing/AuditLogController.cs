using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Auditing.Features.GetAuditLogQuery;
using TestPlatform.Contracts.Auditing.DTOs;
using TestPlatform.Contracts.Authorization;

namespace TestPlatform.Api.Auditing;

[ApiController]
[Route("audit-log")]
[Authorize(Policy = AuthorizationPolicies.ManageSystem)]
public sealed class AuditLogController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AuditLogPageResponse>> Get(
        [FromServices] IQueryHandler<GetAuditLogQuery, AuditLogPageResponse> handler,
        [FromQuery] string? employeeNumber,
        [FromQuery] string? method,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.Handle(
            new GetAuditLogQuery(employeeNumber, method, page, pageSize),
            cancellationToken);
        return result.Value;
    }
}
