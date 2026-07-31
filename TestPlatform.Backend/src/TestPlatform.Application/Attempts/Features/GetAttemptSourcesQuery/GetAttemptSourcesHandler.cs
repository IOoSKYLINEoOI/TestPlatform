using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Attempts.DTOs;
using TestPlatform.Contracts.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Features.GetAttemptSourcesQuery;

public sealed record GetAttemptSourcesQuery(
    string? Search,
    AttemptTypeDto? Type,
    int Page,
    int PageSize) : IQuery;

public sealed class GetAttemptSourcesHandler(
    IAttemptSourcesReadDbContext sourcesDbContext,
    ICurrentUserAccessor currentUserAccessor)
    : IQueryHandler<GetAttemptSourcesQuery, AttemptSourcePageResponse>
{
    public async Task<Result<AttemptSourcePageResponse>> Handle(
        GetAttemptSourcesQuery query,
        CancellationToken cancellationToken)
    {
        var user = currentUserAccessor.User;
        if (user is null)
        {
            return Result.Failure<AttemptSourcePageResponse>(ErrorCodes.Unauthorized);
        }

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var search = query.Search?.Trim();

        var tests = sourcesDbContext.ReadTests.AsNoTracking();
        var exams = sourcesDbContext.ReadExams.AsNoTracking();
        if (!user.IsAdmin)
        {
            tests = tests.Where(item => item.AuthorId == user.Id);
            exams = exams.Where(item => item.AuthorId == user.Id);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            tests = tests.Where(item => item.Title.Contains(search));
            exams = exams.Where(item => item.Title.Contains(search));
        }

        var testRows = tests.Select(item => new AttemptSourceRow
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            Type = (int)AttemptTypeDto.Test,
            Status = (int)item.Status,
            SortAt = item.UpdatedAt,
        });
        var examRows = exams.Select(item => new AttemptSourceRow
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            Type = (int)AttemptTypeDto.Exam,
            Status = (int)item.Status,
            SortAt = item.CreatedAt,
        });

        IQueryable<AttemptSourceRow> sources = query.Type switch
        {
            AttemptTypeDto.Test => testRows,
            AttemptTypeDto.Exam => examRows,
            _ => testRows.Concat(examRows),
        };

        var totalCount = await sources.CountAsync(cancellationToken);
        var rows = await sources
            .OrderByDescending(item => item.SortAt)
            .ThenByDescending(item => item.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var items = rows.Select(item => new AttemptSourceListItemResponse(
            item.Id,
            item.Title,
            item.Description,
            (AttemptTypeDto)item.Type,
            StatusName((AttemptTypeDto)item.Type, item.Status)))
            .ToList();

        return Result.Success(new AttemptSourcePageResponse(items, page, pageSize, totalCount));
    }

    private static string StatusName(AttemptTypeDto type, int status) => type == AttemptTypeDto.Test
        ? ((TestPlatform.Core.Tests.Enums.TestStatus)status).ToString()
        : ((TestPlatform.Core.Exams.Enums.ExamStatus)status).ToString();

    private sealed class AttemptSourceRow
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int Type { get; init; }
        public int Status { get; init; }
        public DateTime SortAt { get; init; }
    }
}
