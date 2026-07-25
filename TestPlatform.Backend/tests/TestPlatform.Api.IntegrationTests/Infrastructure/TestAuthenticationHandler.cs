using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TestPlatform.Api.IntegrationTests.Infrastructure;

public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "IntegrationTest";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Request.Headers.ContainsKey("X-Skip-Test-Authentication"))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var role = Request.Headers.TryGetValue("X-Test-Role", out var roleHeader)
            ? roleHeader.ToString()
            : "Teacher";
        var subject = Request.Headers.TryGetValue("X-Test-Subject", out var subjectHeader)
            ? subjectHeader.ToString()
            : "integration-test-teacher";
        var employeeNumber = Request.Headers.TryGetValue("X-Test-Employee-Number", out var employeeHeader)
            ? employeeHeader.ToString()
            : "TEST-001";

        var claims = new List<Claim> { new(ClaimTypes.Role, role) };
        if (!Request.Headers.ContainsKey("X-Test-Omit-Subject"))
        {
            claims.Add(new Claim("sub", subject));
        }

        if (!Request.Headers.ContainsKey("X-Test-Omit-Employee-Number"))
        {
            claims.Add(new Claim("employee_number", employeeNumber));
        }
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(principal, SchemeName)));
    }
}
