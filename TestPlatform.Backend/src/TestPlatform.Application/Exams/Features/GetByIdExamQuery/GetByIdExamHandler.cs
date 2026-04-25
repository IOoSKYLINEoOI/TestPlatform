using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Exams.DTOs;

namespace TestPlatform.Application.Exams.Features.GetByIdExamQuery;

public record GetByIdExamQuery(Guid Id) : IQuery;

public class GetByIdExamHandler : IQueryHandler<ExamFullResponse, GetByIdExamQuery>
{
    private readonly IExamsReadDbContext _examsDbContext;
    private readonly ILogger<GetByIdExamHandler> _logger;

    public GetByIdExamHandler(IExamsReadDbContext examsDbContext, ILogger<GetByIdExamHandler> logger)
    {
        _examsDbContext = examsDbContext;
        _logger = logger;
    }

    public async Task<Result<ExamFullResponse>> Handle(GetByIdExamQuery query, CancellationToken cancellationToken)
    {
        var response = await _examsDbContext.ReadExams
            .Where(x => x.Id == query.Id)
            .Select(x => new ExamFullResponse(
                Id: x.Id,
                Title: x.Title,
                Description: x.Description,
                TimeLimitSeconds: x.TimeLimitSeconds,
                CoverImageName: x.CoverImageName,
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

        if (response == null)
        {
            _logger.LogWarning("Test with id {Id} not found", query.Id);
            return Result.Failure<ExamFullResponse>("Exam not found");
        }

        _logger.LogInformation("Get Exam with id {Id}", query.Id);
        return Result.Success(response);
    }
}