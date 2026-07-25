using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Contracts.Exams.DTOs;
using TestPlatform.Application.Users;

namespace TestPlatform.Application.Exams.Features.GetByIdExamQuery;

public record GetByIdExamQuery(Guid Id) : IQuery;

public sealed class GetByIdExamHandler(
    IExamsReadDbContext examsDbContext,
    ICurrentUserAccessor currentUserAccessor)
    : IQueryHandler<GetByIdExamQuery, ExamFullResponse>
{
    public async Task<Result<ExamFullResponse>> Handle(GetByIdExamQuery query, CancellationToken cancellationToken)
    {
        var user = currentUserAccessor.User;
        if (user is null)
        {
            return Result.Failure<ExamFullResponse>(ErrorCodes.Unauthorized);
        }

        var exam = await examsDbContext.ReadExams
            .AsNoTracking()
            .Include(item => item.Sections)
            .ThenInclude(section => section.Questions)
            .FirstOrDefaultAsync(
                item => item.Id == query.Id && (item.AuthorId == user.Id || user.IsAdmin),
                cancellationToken);

        if (exam is null)
        {
            return Result.Failure<ExamFullResponse>(ErrorCodes.ExamNotFound);
        }

        return Result.Success(new ExamFullResponse(
            exam.Id,
            exam.Title,
            exam.Description,
            exam.TimeLimitSeconds,
            exam.CoverImageId,
            exam.AuthorId,
            exam.Status.ToString(),
            exam.AttemptsLimit,
            (ExamReviewPolicyDto)exam.ReviewPolicy,
            exam.TotalQuestions,
            exam.TotalMaxScore,
            exam.CreatedAt,
            exam.PublishedAt,
            exam.Schedule is null ? null : new ExamScheduleResponse(exam.Schedule.AvailableFrom, exam.Schedule.AvailableTo),
            exam.PassingRule is null ? null : new PassingRuleResponse(exam.PassingRule.MinScore, exam.PassingRule.MinPercent),
            exam.Sections.Select(section => new ExamSectionResponse(
                section.Id,
                section.Name,
                section.QuestionsToSelect,
                section.ScorePerQuestion,
                section.MaxScore,
                section.QuestionIds)).ToList()));
    }
}
