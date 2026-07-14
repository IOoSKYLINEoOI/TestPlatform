using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Exams;
using TestPlatform.Application.Questions;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Services.SourceService;

public class ExamAttemptSource(
    IExamsReadDbContext examsReadDbContext,
    IQuestionsReadDbContext questionsReadDbContext) : IAttemptSourceService
{
    public AttemptType Type => AttemptType.Exam;

    public async Task<Result<AttemptSource>> GetSourceAsync(Guid sourceId, CancellationToken cancellationToken)
    {
        var exam = await examsReadDbContext.ReadExams
            .AsNoTracking()
            .Where(t => t.Id == sourceId)
            .Select(t => new
            {
                t.TimeLimitSeconds,
                Questions = t.Questions
                    .OrderBy(q => q.Order)
                    .Select(q => new { q.QuestionId, q.Order, q.Score, })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (exam is null)
            return Result.Failure<AttemptSource>(ErrorCodes.ExamNotFound);

        var questionIds = exam.Questions
            .Select(q => q.QuestionId)
            .ToList();

        var questions = await questionsReadDbContext.ReadQuestions
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .Include(q => q.Tags)
            .ToListAsync(cancellationToken);

        var questionMap = questions.ToDictionary(q => q.Id);

        var attemptQuestions = exam.Questions
            .Select(q => new AttemptQuestion(
                q.Order,
                q.Score,
                questionMap[q.QuestionId]))
            .ToList();

        var source = new AttemptSource(
            attemptQuestions,
            attemptQuestions.Count,
            attemptQuestions.Sum(q => q.Score),
            exam.TimeLimitSeconds);

        return Result.Success(source);
    }
}