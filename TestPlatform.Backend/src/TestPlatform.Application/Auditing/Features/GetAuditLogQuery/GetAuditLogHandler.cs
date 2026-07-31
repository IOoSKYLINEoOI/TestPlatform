using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Auditing.DTOs;

namespace TestPlatform.Application.Auditing.Features.GetAuditLogQuery;

public sealed record GetAuditLogQuery(
    string? EmployeeNumber,
    string? Method,
    int Page,
    int PageSize) : IQuery;

public sealed class GetAuditLogHandler(IAuditLogReadDbContext dbContext)
    : IQueryHandler<GetAuditLogQuery, AuditLogPageResponse>
{
    public async Task<Result<AuditLogPageResponse>> Handle(
        GetAuditLogQuery query,
        CancellationToken cancellationToken)
    {
        var rows = dbContext.ReadAuditLog;
        if (!string.IsNullOrWhiteSpace(query.EmployeeNumber))
        {
            var employeeNumber = query.EmployeeNumber.Trim();
            rows = rows.Where(item => item.EmployeeNumber != null &&
                                      item.EmployeeNumber.Contains(employeeNumber));
        }
        if (!string.IsNullOrWhiteSpace(query.Method))
        {
            var method = query.Method.Trim().ToUpperInvariant();
            rows = rows.Where(item => item.Method == method);
        }

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var totalCount = await rows.CountAsync(cancellationToken);
        var items = await rows
            .OrderByDescending(item => item.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(item => new AuditLogItemResponse(
                item.Id, item.UserId, item.EmployeeNumber, item.Method,
                item.Path, item.StatusCode, item.TraceId, item.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new AuditLogPageResponse(items, page, pageSize, totalCount));
    }
}
