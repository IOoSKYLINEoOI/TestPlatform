using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Contracts.Exams.DTOs;

namespace TestPlatform.Application.Exams.Features.GetByIdExamQuery;

public record GetByIdExamQuery(Guid Id) : IQuery;

public class GetByIdExamHandler(IExamsReadDbContext examsDbContext) : IQueryHandler<GetByIdExamQuery, ExamFullResponse>
{
    public async Task<Result<ExamFullResponse>> Handle(GetByIdExamQuery query, CancellationToken cancellationToken)
    {
        var response = await examsDbContext.ReadExams
            .Where(x => x.Id == query.Id)
            .Select(x => new ExamFullResponse(
                Id: x.Id,
                Title: x.Title,
                Description: x.Description,
                TimeLimitSeconds: x.TimeLimitSeconds,
                CoverImageId: x.CoverImageId,
                AuthorId: x.AuthorId,
                Status: x.Status.ToString(),
                CreatedAt: x.CreatedAt,
                PublishedAt: x.PublishedAt,
                Schedule: x.Schedule == null ? null : new ExamScheduleResponse(x.Schedule.AvailableFrom, x.Schedule.AvailableTo),
                PassingRule: x.PassingRule == null ? null : new PassingRuleResponse(x.PassingRule.MinScore, x.PassingRule.MinPercent),
                Questions: x.Questions
                    .OrderBy(q => q.Order)
                    .Select(q => new ExamQuestionResponse(
                        q.QuestionId,
                        q.Order,
                        q.Score))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        return response is null
            ? Result.Failure<ExamFullResponse>(ErrorCodes.ExamNotFound)
            : Result.Success(response);
    }
}