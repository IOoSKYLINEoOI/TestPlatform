using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Mappers;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Exams;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Attempts.Enums;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Features.GetExamAttemptsQuery;

public record GetExamAttemptsQuery(
    Guid ExamId,
    AttemptStatusDto? Status,
    bool? Passed,
    string? EmployeeNumber,
    int Page,
    int PageSize) : IQuery;

public class GetExamAttemptsHandler(
    IAttemptsReadDbContext attemptsDbContext,
    IExamsReadDbContext examsDbContext,
    IUsersReadDbContext usersDbContext,
    ICurrentUserAccessor currentUserAccessor)
    : IQueryHandler<GetExamAttemptsQuery, ExamAttemptsPageResponse>
{
    public async Task<Result<ExamAttemptsPageResponse>> Handle(
        GetExamAttemptsQuery query,
        CancellationToken cancellationToken)
    {
        var user = currentUserAccessor.User;
        if (user is null)
        {
            return Result.Failure<ExamAttemptsPageResponse>(ErrorCodes.Unauthorized);
        }

        var exam = await examsDbContext.ReadExams
            .Where(x => x.Id == query.ExamId)
            .Select(x => new { x.AuthorId })
            .FirstOrDefaultAsync(cancellationToken);
        if (exam is null)
        {
            return Result.Failure<ExamAttemptsPageResponse>(ErrorCodes.ExamNotFound);
        }

        if (exam.AuthorId != user.Id && !user.IsAdmin)
        {
            return Result.Failure<ExamAttemptsPageResponse>(ErrorCodes.Forbidden);
        }

        var attempts = attemptsDbContext.ReadAttempts
            .AsNoTracking()
            .Where(x => x.Type == AttemptType.Exam && x.SourceId == query.ExamId);

        if (query.Status.HasValue)
        {
            var status = query.Status.Value.ToDomain();
            attempts = attempts.Where(x => x.Status == status);
        }

        if (query.Passed.HasValue)
        {
            attempts = attempts.Where(x => x.AttemptResult != null && x.AttemptResult.Passed == query.Passed);
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
            return new ExamAttemptListItemResponse(
                attempt.Id,
                attempt.AttemptNumber,
                attempt.UserId,
                employeeNumbers.GetValueOrDefault(attempt.UserId, string.Empty),
                attempt.Status.ToDto(),
                attempt.TotalQuestions,
                attempt.AnsweredQuestions,
                result?.EarnedPoints,
                attempt.TotalMaxScore,
                result is null ? null : (double)(result.EarnedPoints / attempt.TotalMaxScore * 100),
                result?.Passed,
                attempt.StartedAt,
                attempt.FinishedAt);
        }).ToList();

        return Result.Success(new ExamAttemptsPageResponse(items, page, pageSize, totalCount));
    }
}
