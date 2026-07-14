using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Contracts.Tests.DTOs;

namespace TestPlatform.Application.Tests.Features.GetByIdTestQuery;

public record GetByIdTestQuery(Guid Id) : IQuery;

public class GetByIdTestHandler(ITestsReadDbContext testsDbContext) : IQueryHandler<GetByIdTestQuery, TestFullResponse>
{
    public async Task<Result<TestFullResponse>> Handle(GetByIdTestQuery query, CancellationToken cancellationToken)
    {
        var response = await testsDbContext.ReadTests
            .Where(t => t.Id == query.Id)
            .Select(t => new TestFullResponse(
                t.Id,
                t.Title,
                t.Description,
                t.TimeLimitSeconds,
                t.CoverImageName,
                t.AuthorId,
                t.CreatedAt,
                t.Questions
                    .OrderBy(q => q.Order)
                    .Select(q => new TestQuestionResponse(
                        q.QuestionId,
                        q.Order,
                        q.Score))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        return response is null
            ? Result.Failure<TestFullResponse>(ErrorCodes.TestNotFound)
            : Result.Success(response);
    }
}