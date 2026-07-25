using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Mappers;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Tests;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Attempts.Enums;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Features.GetTestAttemptsQuery;

public record GetTestAttemptsQuery(
    Guid TestId,
    AttemptStatusDto? Status,
    string? EmployeeNumber,
    int Page,
    int PageSize) : IQuery;

public sealed class GetTestAttemptsHandler(
    IAttemptsReadDbContext attemptsDbContext,
    ITestsReadDbContext testsDbContext,
    IUsersReadDbContext usersDbContext,
    ICurrentUserAccessor currentUserAccessor)
    : IQueryHandler<GetTestAttemptsQuery, TestAttemptsPageResponse>
{
    public async Task<Result<TestAttemptsPageResponse>> Handle(
        GetTestAttemptsQuery query,
        CancellationToken cancellationToken)
    {
        var user = currentUserAccessor.User;
        if (user is null)
        {
            return Result.Failure<TestAttemptsPageResponse>(ErrorCodes.Unauthorized);
        }

        var test = await testsDbContext.ReadTests
            .Where(x => x.Id == query.TestId)
            .Select(x => new { x.AuthorId })
            .FirstOrDefaultAsync(cancellationToken);
        if (test is null)
        {
            return Result.Failure<TestAttemptsPageResponse>(ErrorCodes.TestNotFound);
        }

        if (test.AuthorId != user.Id && !user.IsAdmin)
        {
            return Result.Failure<TestAttemptsPageResponse>(ErrorCodes.Forbidden);
        }

        var attempts = attemptsDbContext.ReadAttempts
            .AsNoTracking()
            .Where(x => x.Type == AttemptType.Test && x.SourceId == query.TestId);

        if (query.Status.HasValue)
        {
            attempts = attempts.Where(x => x.Status == query.Status.Value.ToDomain());
        }

        if (!string.IsNullOrWhiteSpace(query.EmployeeNumber))
        {
            var employeeNumber = query.EmployeeNumber.Trim();
            var matchingUserIds = await usersDbContext.ReadUsers
                .Where(x => x.EmployeeNumber.Contains(employeeNumber))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
            attempts = attempts.Where(x => matchingUserIds.Contains(x.UserId));
        }

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var totalCount = await attempts.CountAsync(cancellationToken);
        var rows = await attempts
            .OrderByDescending(x => x.StartedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = rows.Select(x => x.UserId).Distinct().ToArray();
        var employeeNumbers = await usersDbContext.ReadUsers
            .Where(x => userIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.EmployeeNumber, cancellationToken);

        var items = rows.Select(attempt =>
        {
            var result = attempt.AttemptResult;
            return new TestAttemptListItemResponse(
                attempt.Id,
                attempt.AttemptNumber,
                attempt.UserId,
                employeeNumbers.GetValueOrDefault(attempt.UserId, string.Empty),
                attempt.Status.ToDto(),
                attempt.TotalQuestions,
                attempt.AnsweredQuestions,
                result?.CorrectAnswers,
                result is null ? null : (double)result.CorrectAnswers / attempt.TotalQuestions * 100,
                attempt.StartedAt,
                attempt.FinishedAt);
        }).ToList();

        return Result.Success(new TestAttemptsPageResponse(items, page, pageSize, totalCount));
    }
}
