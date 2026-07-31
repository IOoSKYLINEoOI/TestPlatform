using TestPlatform.Application.Users;
using TestPlatform.Core.Auditing;
using TestPlatform.Infrastructure.Postgres;

namespace TestPlatform.Web.Auditing;

public sealed class AuditLogMiddleware(RequestDelegate next, ILogger<AuditLogMiddleware> logger)
{
    public async Task InvokeAsync(
        HttpContext context,
        TestPlatformDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor)
    {
        await next(context);

        if (HttpMethods.IsGet(context.Request.Method) ||
            HttpMethods.IsHead(context.Request.Method) ||
            HttpMethods.IsOptions(context.Request.Method))
        {
            return;
        }

        try
        {
            var identity = currentUserAccessor.User;
            dbContext.AuditLog.Add(new AuditLogEntry(
                Guid.NewGuid(),
                identity?.Id,
                identity?.EmployeeNumber,
                context.Request.Method,
                context.Request.Path.Value ?? "/",
                context.Response.StatusCode,
                context.TraceIdentifier,
                DateTime.UtcNow));
            await dbContext.SaveChangesAsync(context.RequestAborted);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to persist audit log entry for {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
    }
}
