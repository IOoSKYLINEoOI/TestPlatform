using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Users;
using TestPlatform.Core.Users;

namespace TestPlatform.Infrastructure.Identity;

public class CurrentIdentityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CurrentIdentityMiddleware> _logger;

    public CurrentIdentityMiddleware(
        RequestDelegate next,
        ILogger<CurrentIdentityMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IUsersReadDbContext usersReadDbContext,
        IUsersRepository usersRepository,
        IUnitOfWork unitOfWork)
    {
        var principal = context.User;
        if (principal.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var keycloakId = principal.FindFirstValue(KeycloakClaimNames.Subject)?.Trim();
        var employeeNumber = principal.FindFirstValue(KeycloakClaimNames.EmployeeNumber)?.Trim();

        if (string.IsNullOrWhiteSpace(keycloakId) || string.IsNullOrWhiteSpace(employeeNumber))
        {
            _logger.LogWarning(
                "Authenticated Keycloak user does not have required identity claims. HasSub: {HasSub}; HasEmployeeNumber: {HasEmployeeNumber}",
                !string.IsNullOrWhiteSpace(keycloakId),
                !string.IsNullOrWhiteSpace(employeeNumber));

            await WriteForbiddenAsync(context, "identity.required_claim_missing");
            return;
        }

        var userInfo = await usersReadDbContext.ReadUsers
            .AsNoTracking()
            .Where(x => x.KeycloakId == keycloakId)
            .Select(x => new { x.Id, x.KeycloakId, x.EmployeeNumber })
            .FirstOrDefaultAsync(context.RequestAborted);

        if (userInfo is null)
        {
            var createResult = User.Create(keycloakId, employeeNumber);
            if (createResult.IsFailure)
            {
                _logger.LogError("Failed to provision local user. Error: {Error}", createResult.Error);
                await WriteForbiddenAsync(context, "identity.user_provisioning_failed");
                return;
            }

            await usersRepository.AddAsync(createResult.Value, context.RequestAborted);

            try
            {
                await unitOfWork.SaveChangesAsync(context.RequestAborted);
            }
            catch (DbUpdateException)
            {
                usersRepository.Detach(createResult.Value);
                _logger.LogWarning(
                    "Concurrent local-user provisioning detected for Keycloak user {KeycloakUserId}",
                    keycloakId);

                userInfo = await usersReadDbContext.ReadUsers
                    .AsNoTracking()
                    .Where(x => x.KeycloakId == keycloakId)
                    .Select(x => new { x.Id, x.KeycloakId, x.EmployeeNumber })
                    .FirstOrDefaultAsync(context.RequestAborted);

                if (userInfo is null)
                {
                    await WriteForbiddenAsync(context, "identity.user_provisioning_failed");
                    return;
                }
            }

            userInfo ??= new
            {
                createResult.Value.Id,
                createResult.Value.KeycloakId,
                createResult.Value.EmployeeNumber,
            };
        }

        if (!string.Equals(userInfo.EmployeeNumber, employeeNumber, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "Employee number mismatch for Keycloak user {KeycloakUserId}",
                keycloakId);

            await WriteForbiddenAsync(context, "identity.employee_number_mismatch");
            return;
        }

        CurrentIdentityHttpContext.Set(
            context,
            new CurrentIdentity(
                userInfo.Id,
                userInfo.EmployeeNumber,
                principal.IsInRole("Admin")));

        await _next(context);
    }

    private static async Task WriteForbiddenAsync(HttpContext context, string error)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Access is forbidden.",
        };
        problem.Extensions["code"] = error;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        await context.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: context.RequestAborted);
    }
}
