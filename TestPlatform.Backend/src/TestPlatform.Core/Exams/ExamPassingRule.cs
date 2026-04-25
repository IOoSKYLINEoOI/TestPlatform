using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Exams;

public class ExamPassingRule
{
    public int? MinScore { get; }

    public double? MinPercent { get; }

    private ExamPassingRule() { }

    private ExamPassingRule(int? minScore, double? minPercent)
    {
        MinScore = minScore;
        MinPercent = minPercent;
    }

    public static Result<ExamPassingRule> Create(int? minScore, double? minPercent)
    {
        if (minScore is null && minPercent is null)
            return Result.Failure<ExamPassingRule>("Нужно задать хотя бы одно условие прохождения");

        if (minPercent is < 0 or > 100)
            return Result.Failure<ExamPassingRule>("Процент должен быть от 0 до 100");

        if (minScore is < 0)
            return Result.Failure<ExamPassingRule>("Баллы не могут быть отрицательными");

        return Result.Success(new ExamPassingRule(minScore, minPercent));
    }
}
