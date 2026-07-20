/*
using CSharpFunctionalExtensions;
using TestPlatform.Core.Questions;

namespace TestPlatform.Core.Attempts.Snapshot;

public class AttemptSnapshot
{
    private readonly IReadOnlyList<AttemptQuestionSnapshot> _questions;

    private AttemptSnapshot(IEnumerable<AttemptQuestionSnapshot> questions)
    {
        _questions = questions
            .OrderBy(q => q.Order)
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyList<AttemptQuestionSnapshot> Questions => _questions;

    public static Result<AttemptSnapshot> Create(IEnumerable<AttemptQuestionSnapshot> questions)
    {
        var list = questions.ToList();

        if (!list.Any())
            return Result.Failure<AttemptSnapshot>("Должен быть хотя бы один вопрос");

        var duplicates = list
            .GroupBy(q => q.QuestionId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Any())
            return Result.Failure<AttemptSnapshot>("Обнаружены дублирующиеся вопросы");

        return Result.Success(new AttemptSnapshot(list));
    }

    public Result<AttemptResult> CalculateResult(IEnumerable<AttemptAnswer> answers)
    {
        decimal earnedPoints = 0;
        int correctAnswers = 0;

        foreach (var answer in answers)
        {
            var question = Questions
                .FirstOrDefault(x => x.QuestionId == answer.QuestionId);

            if (question is null)
                return Result.Failure<AttemptResult>("Вопрос не найден");

            var scoreResult = question.CalculateScore(answer);

            if (scoreResult.IsFailure)
                return Result.Failure<AttemptResult>(scoreResult.Error);

            var score = scoreResult.Value;

            earnedPoints += score;

            if (score > 0)
                correctAnswers++;
        }

        return Result.Success(new AttemptResult(correctAnswers, earnedPoints));
    }
}
*/