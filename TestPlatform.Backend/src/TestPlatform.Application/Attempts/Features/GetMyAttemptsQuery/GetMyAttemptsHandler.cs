using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Mappers;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Exams;
using TestPlatform.Application.Tests;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Features.GetMyAttemptsQuery;

public record GetMyAttemptsQuery(
    AttemptTypeDto? Type,
    AttemptStatusDto? Status,
    int Page,
    int PageSize) : IQuery;

public class GetMyAttemptsHandler(
    IAttemptsReadDbContext attemptsDbContext,
    ITestsReadDbContext testsDbContext,
    IExamsReadDbContext examsDbContext,
    ICurrentUserAccessor currentUserAccessor)
    : IQueryHandler<GetMyAttemptsQuery, AttemptHistoryPageResponse>
{
    public async Task<Result<AttemptHistoryPageResponse>> Handle(
        GetMyAttemptsQuery query,
        CancellationToken cancellationToken)
    {
        var user = currentUserAccessor.User;
        if (user is null)
        {
            return Result.Failure<AttemptHistoryPageResponse>(ErrorCodes.Unauthorized);
        }

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var attempts = attemptsDbContext.ReadAttempts
            .AsNoTracking()
            .Where(x => x.UserId == user.Id);

        if (query.Type.HasValue)
        {
            var type = query.Type.Value.ToDomain();
            attempts = attempts.Where(x => x.Type == type);
        }

        if (query.Status.HasValue)
        {
            var status = query.Status.Value.ToDomain();
            attempts = attempts.Where(x => x.Status == status);
        }

        var totalCount = await attempts.CountAsync(cancellationToken);
        var items = await attempts
            .OrderByDescending(x => x.StartedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var testIds = items
            .Where(x => x.Type == TestPlatform.Core.Attempts.Enums.AttemptType.Test)
            .Select(x => x.SourceId)
            .Distinct()
            .ToArray();
        var examIds = items
            .Where(x => x.Type == TestPlatform.Core.Attempts.Enums.AttemptType.Exam)
            .Select(x => x.SourceId)
            .Distinct()
            .ToArray();
        var testTitles = await testsDbContext.ReadTests
            .Where(x => testIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Title, cancellationToken);
        var examTitles = await examsDbContext.ReadExams
            .Where(x => examIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Title, cancellationToken);
        var sourceTitles = testTitles.Concat(examTitles).ToDictionary(x => x.Key, x => x.Value);

        return Result.Success(new AttemptHistoryPageResponse(
            items.Select(x => x.ToHistoryResponse(
                sourceTitles.GetValueOrDefault(x.SourceId, "Удалённый материал"))).ToList(),
            page,
            pageSize,
            totalCount));
    }
}
