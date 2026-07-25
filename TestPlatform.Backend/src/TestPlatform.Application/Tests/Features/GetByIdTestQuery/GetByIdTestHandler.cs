using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Contracts.Tests.DTOs;
using TestPlatform.Contracts.Tests.Enums;
using TestPlatform.Application.Users;

namespace TestPlatform.Application.Tests.Features.GetByIdTestQuery;

public record GetByIdTestQuery(Guid Id) : IQuery;

public class GetByIdTestHandler(
    ITestsReadDbContext testsDbContext,
    ICurrentUserAccessor currentUserAccessor) : IQueryHandler<GetByIdTestQuery, TestFullResponse>
{
    public async Task<Result<TestFullResponse>> Handle(GetByIdTestQuery query, CancellationToken cancellationToken)
    {
        var user = currentUserAccessor.User;
        if (user is null)
        {
            return Result.Failure<TestFullResponse>(ErrorCodes.Unauthorized);
        }

        var response = await testsDbContext.ReadTests
            .Where(t => t.Id == query.Id && (t.AuthorId == user.Id || user.IsAdmin))
            .Select(t => new TestFullResponse(
                t.Id,
                t.Title,
                t.Description,
                t.TimeLimitSeconds,
                t.CoverImageId,
                t.AuthorId,
                t.CreatedAt,
                t.UpdatedAt,
                t.PublishedAt,
                (TestStatusDto)t.Status,
                t.Questions
                    .OrderBy(q => q.Order)
                    .Select(q => new TestQuestionResponse(
                        q.QuestionId,
                        q.Order))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        return response is null
            ? Result.Failure<TestFullResponse>(ErrorCodes.TestNotFound)
            : Result.Success(response);
    }
}
