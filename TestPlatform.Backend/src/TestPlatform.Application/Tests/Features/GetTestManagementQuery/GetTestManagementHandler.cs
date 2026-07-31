using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Tests.DTOs;
using TestPlatform.Contracts.Tests.Enums;

namespace TestPlatform.Application.Tests.Features.GetTestManagementQuery;

public sealed record GetTestManagementQuery(string? Search, int Page, int PageSize) : IQuery;

public sealed class GetTestManagementHandler(ITestsReadDbContext testsDbContext)
    : IQueryHandler<GetTestManagementQuery, TestManagementPageResponse>
{
    public async Task<Result<TestManagementPageResponse>> Handle(
        GetTestManagementQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var tests = testsDbContext.ReadTests.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            tests = tests.Where(test => test.Title.Contains(search));
        }

        var totalCount = await tests.CountAsync(cancellationToken);
        var items = await tests
            .OrderByDescending(test => test.UpdatedAt)
            .ThenByDescending(test => test.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(test => new TestResponse(
                test.Id,
                test.Title,
                test.Description,
                test.TimeLimitSeconds,
                test.AuthorId,
                test.CreatedAt,
                test.UpdatedAt,
                (TestStatusDto)test.Status,
                test.Questions.Count))
            .ToListAsync(cancellationToken);

        return Result.Success(new TestManagementPageResponse(items, page, pageSize, totalCount));
    }
}
