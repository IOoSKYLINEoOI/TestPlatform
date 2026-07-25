using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Exams;
using TestPlatform.Application.Questions;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;
using TestPlatform.Core.Exams.Enums;

namespace TestPlatform.Application.Attempts.Services.SourceService;

public sealed class ExamAttemptSource(
    IExamsReadDbContext examsReadDbContext,
    IQuestionsReadDbContext questionsReadDbContext) : IAttemptSourceService
{
    public AttemptType Type => AttemptType.Exam;

    public async Task<Result<AttemptSource>> GetSourceAsync(Guid sourceId, CancellationToken cancellationToken)
    {
        var exam = await examsReadDbContext.ReadExams
            .AsNoTracking()
            .Include(item => item.Sections)
            .ThenInclude(section => section.Questions)
            .FirstOrDefaultAsync(item => item.Id == sourceId, cancellationToken);

        if (exam is null)
        {
            return Result.Failure<AttemptSource>(ErrorCodes.ExamNotFound);
        }

        if (exam.Status != ExamStatus.Published)
        {
            return Result.Failure<AttemptSource>("exam.not_published");
        }

        var now = DateTime.UtcNow;
        if (exam.Schedule?.AvailableFrom > now)
        {
            return Result.Failure<AttemptSource>("exam.not_started");
        }

        if (exam.Schedule?.AvailableTo < now)
        {
            return Result.Failure<AttemptSource>("exam.finished");
        }

        var selected = exam.Sections
            .SelectMany(section => section.QuestionIds
                .OrderBy(_ => Random.Shared.Next())
                .Take(section.QuestionsToSelect)
                .Select(questionId => new { QuestionId = questionId, Score = section.ScorePerQuestion }))
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        var questionIds = selected.Select(item => item.QuestionId).ToList();
        var questions = await questionsReadDbContext.ReadQuestions
            .AsNoTracking()
            .Where(question => questionIds.Contains(question.Id))
            .Include(question => question.Tags)
            .ToListAsync(cancellationToken);
        var questionMap = questions.ToDictionary(question => question.Id);

        if (questionMap.Count != questionIds.Count)
        {
            return Result.Failure<AttemptSource>("exam.question_pool_incomplete");
        }

        var attemptQuestions = selected
            .Select((item, index) => new AttemptQuestion(index + 1, item.Score, questionMap[item.QuestionId]))
            .ToList();

        return Result.Success(new AttemptSource(
            attemptQuestions,
            exam.TotalQuestions,
            exam.TotalMaxScore,
            exam.TimeLimitSeconds,
            exam.AttemptsLimit,
            exam.PassingRule?.MinScore,
            exam.PassingRule?.MinPercent,
            exam.Schedule?.AvailableTo,
            exam.ReviewPolicy == ExamReviewPolicy.AfterExamClosed
                ? exam.Schedule?.AvailableTo
                : null));
    }
}
