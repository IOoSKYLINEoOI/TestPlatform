using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Exams.DTOs;
using TestPlatform.Core.Exams.Enums;

namespace TestPlatform.Application.Exams.Features.GetExamCatalogQuery;

public record GetExamCatalogQuery(int Page, int PageSize) : IQuery;

public sealed class GetExamCatalogHandler(IExamsReadDbContext examsDbContext)
    : IQueryHandler<GetExamCatalogQuery, ExamCatalogPageResponse>
{
    public async Task<Result<ExamCatalogPageResponse>> Handle(
        GetExamCatalogQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var exams = examsDbContext.ReadExams
            .AsNoTracking()
            .Where(x => x.Status == ExamStatus.Published);
        var totalCount = await exams.CountAsync(cancellationToken);
        var rows = await exams
            .Include(x => x.Sections)
            .OrderByDescending(x => x.PublishedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = rows.Select(x => new ExamCatalogItemResponse(
            x.Id,
            x.Title,
            x.Description,
            x.CoverImageId,
            x.TimeLimitSeconds,
            x.AttemptsLimit,
            x.TotalQuestions,
            x.TotalMaxScore,
            x.Schedule?.AvailableFrom,
            x.Schedule?.AvailableTo)).ToList();

        return Result.Success(new ExamCatalogPageResponse(items, page, pageSize, totalCount));
    }
}
