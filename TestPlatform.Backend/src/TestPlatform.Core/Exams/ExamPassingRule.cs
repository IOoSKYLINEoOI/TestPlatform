using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Exams;

public class ExamPassingRule
{
    public int? MinScore { get; private set; }

    public double? MinPercent { get; private set; }

    private ExamPassingRule() { }

    private ExamPassingRule(int? minScore, double? minPercent)
    {
        MinScore = minScore;
        MinPercent = minPercent;
    }

    public static Result<ExamPassingRule> Create(int? minScore, double? minPercent)
    {
        if (minScore.HasValue == minPercent.HasValue)
        {
            return Result.Failure<ExamPassingRule>("exam.passing_rule.exactly_one_required");
        }

        if (minPercent is <= 0 or > 100)
        {
            return Result.Failure<ExamPassingRule>("exam.passing_rule.invalid_percent");
        }

        if (minScore is <= 0)
        {
            return Result.Failure<ExamPassingRule>("exam.passing_rule.invalid_score");
        }

        return Result.Success(new ExamPassingRule(minScore, minPercent));
    }
}
