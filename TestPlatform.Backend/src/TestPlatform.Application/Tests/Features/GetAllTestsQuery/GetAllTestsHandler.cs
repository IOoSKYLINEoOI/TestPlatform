using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Tests.DTOs;

namespace TestPlatform.Application.Tests.Features.GetAllTestsQuery;

public record GetAllTestsQuery() : IQuery;

public class GetAllTestsHandler(ITestsReadDbContext testsDbContext) : IQueryHandler<GetAllTestsQuery, IReadOnlyList<TestResponse>>
{
    public async Task<Result<IReadOnlyList<TestResponse>>> Handle(GetAllTestsQuery query, CancellationToken cancellationToken)
    {
        var response = await testsDbContext.ReadTests
            .Select(t => new TestResponse(
                t.Id,
                t.Title,
                t.Description,
                t.TimeLimitSeconds,
                t.AuthorId,
                t.CreatedAt,
                t.Questions.Count))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<TestResponse>>(response);
    }
}