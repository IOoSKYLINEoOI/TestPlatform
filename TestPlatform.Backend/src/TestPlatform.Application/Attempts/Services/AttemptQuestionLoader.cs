using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Questions;
using TestPlatform.Core.Attempts;

namespace TestPlatform.Application.Attempts.Services;

public class AttemptQuestionLoader(IQuestionsReadDbContext questionsDbContext)
{
    public async Task<Result<IReadOnlyCollection<AttemptQuestion>>> LoadAsync(
        IReadOnlyCollection<AttemptQuestionSelection> selections,
        CancellationToken cancellationToken)
    {
        var questionIds = selections.Select(x => x.QuestionId).ToArray();
        var questions = await questionsDbContext.ReadQuestions
            .Where(x => questionIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (questions.Count != questionIds.Length)
        {
            return Result.Failure<IReadOnlyCollection<AttemptQuestion>>(
                "attempt.questions_not_found");
        }

        var questionMap = questions.ToDictionary(x => x.Id);
        var result = selections
            .OrderBy(x => x.Order)
            .Select(x => new AttemptQuestion(x.Order, x.Score, questionMap[x.QuestionId]))
            .ToList();

        return Result.Success<IReadOnlyCollection<AttemptQuestion>>(result);
    }
}
