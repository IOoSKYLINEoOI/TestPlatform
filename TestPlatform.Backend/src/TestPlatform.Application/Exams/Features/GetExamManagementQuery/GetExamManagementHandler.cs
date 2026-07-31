using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Exams.DTOs;

namespace TestPlatform.Application.Exams.Features.GetExamManagementQuery;

public sealed record GetExamManagementQuery(string? Search, int Page, int PageSize) : IQuery;

public sealed class GetExamManagementHandler(IExamsReadDbContext examsDbContext)
    : IQueryHandler<GetExamManagementQuery, ExamManagementPageResponse>
{
    public async Task<Result<ExamManagementPageResponse>> Handle(
        GetExamManagementQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var exams = examsDbContext.ReadExams.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            exams = exams.Where(x => x.Title.Contains(search));
        }

        var totalCount = await exams.CountAsync(cancellationToken);
        var rows = await exams
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var items = rows
            .Select(x => new ExamManagementItemResponse(
                x.Id,
                x.Title,
                x.Description,
                x.Status.ToString(),
                x.TotalQuestions,
                x.TotalMaxScore,
                x.CreatedAt,
                x.PublishedAt))
            .ToList();

        return Result.Success(new ExamManagementPageResponse(items, page, pageSize, totalCount));
    }
}
