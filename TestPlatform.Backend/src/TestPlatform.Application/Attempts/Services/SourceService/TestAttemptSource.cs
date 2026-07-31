using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Questions;
using TestPlatform.Application.Tests;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;
using TestPlatform.Core.Tests.Enums;

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
                t.Status,
                Questions = t.Questions
                    .OrderBy(q => q.Order)
                    .Select(q => new { q.QuestionId, q.Order })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (test is null)
        {
            return Result.Failure<AttemptSource>(ErrorCodes.TestNotFound);
        }

        if (test.Status != TestStatus.Published)
        {
            return Result.Failure<AttemptSource>("test.not_published");
        }

        var questionIds = test.Questions
            .Select(q => q.QuestionId)
            .ToList();

        var questions = await questionsReadDbContext.ReadQuestions
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .Include(q => q.Tags)
            .ToListAsync(cancellationToken);

        var questionMap = questions.ToDictionary(q => q.Id);

        if (questionMap.Count != questionIds.Count)
        {
            return Result.Failure<AttemptSource>("test.question_list_incomplete");
        }

        var attemptQuestions = test.Questions
            .OrderBy(_ => Random.Shared.Next())
            .Select((q, index) => new AttemptQuestion(
                index + 1,
                1,
                questionMap[q.QuestionId]))
            .ToList();

        var source = new AttemptSource(
            attemptQuestions,
            attemptQuestions.Count,
            attemptQuestions.Count,
            test.TimeLimitSeconds);

        return Result.Success(source);
    }
}
