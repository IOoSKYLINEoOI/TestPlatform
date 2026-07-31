using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Dashboard.Features.GetAdminDashboardQuery;
using TestPlatform.Contracts.Authorization;
using TestPlatform.Contracts.Dashboard.DTOs;

namespace TestPlatform.Api.Dashboard;

[ApiController]
[Route("dashboard")]
[Authorize(Policy = AuthorizationPolicies.ManageSystem)]
public sealed class DashboardController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(AdminDashboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminDashboardResponse>> Get(
        [FromServices] IQueryHandler<GetAdminDashboardQuery, AdminDashboardResponse> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new GetAdminDashboardQuery(), cancellationToken);
        return result.Value;
    }
}
