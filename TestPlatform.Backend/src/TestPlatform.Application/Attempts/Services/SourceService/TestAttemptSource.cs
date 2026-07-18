using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Questions;
using TestPlatform.Application.Tests;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Services.SourceService;

public class TestAttemptSource(
    ITestsReadDbContext testsReadDbContext,
    IQuestionsReadDbContext questionsReadDbContext) : IAttemptSourceService
{
    public AttemptType Type => AttemptType.Test;

    public async Task<Result<AttemptSource>> GetSourceAsync(
        Guid sourceId,
        CancellationToken cancellationToken)
    {
        var test = await testsReadDbContext.ReadTests
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

        if (test is null)
            return Result.Failure<AttemptSource>(ErrorCodes.TestNotFound);

        var questionIds = test.Questions
            .Select(q => q.QuestionId)
            .ToList();

        var questions = await questionsReadDbContext.ReadQuestions
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .Include(q => q.Tags)
            .ToListAsync(cancellationToken);

        var questionMap = questions.ToDictionary(q => q.Id);

        var attemptQuestions = test.Questions
            .Select(q => new AttemptQuestion(
                q.Order,
                q.Score,
                questionMap[q.QuestionId]))
            .ToList();

        var source = new AttemptSource(
            attemptQuestions,
            attemptQuestions.Count,
            attemptQuestions.Sum(q => q.Score),
            test.TimeLimitSeconds);

        return Result.Success(source);
    }
}