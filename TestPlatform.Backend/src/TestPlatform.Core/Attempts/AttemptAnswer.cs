using CSharpFunctionalExtensions;

namespace TestPlatform.Core.Attempts;

public class AttemptAnswer
{
    private readonly List<Guid> _selectedOptionIds = new();
    private readonly List<AttemptMatchingPair> _matchingPairs = new();

    private AttemptAnswer() { }

    private AttemptAnswer(Guid questionId)
    {
        QuestionId = questionId;
    }

    public Guid QuestionId { get; }

    public IReadOnlyCollection<Guid> SelectedOptionIds =>
        _selectedOptionIds.AsReadOnly();

    public string? TextAnswer { get; private set; }

    public decimal? NumberAnswer { get; private set; }

    public IReadOnlyCollection<AttemptMatchingPair> MatchingPairs =>
        _matchingPairs.AsReadOnly();


    public static Result<AttemptAnswer> CreateChoice(
        Guid questionId,
        IEnumerable<Guid> optionIds)
    {
        var ids = optionIds.Distinct().ToList();

        if (ids.Count == 0)
        {
            return Result.Failure<AttemptAnswer>("attempt.answer.choice_required");
        }

        var answer = new AttemptAnswer(questionId);

        answer._selectedOptionIds.AddRange(ids);

        return Result.Success(answer);
    }

    public static Result<AttemptAnswer> CreateText(
        Guid questionId,
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Failure<AttemptAnswer>("attempt.answer.text_required");
        }

        return Result.Success(
            new AttemptAnswer(questionId) { TextAnswer = text.Trim(), });
    }

    public static Result<AttemptAnswer> CreateNumber(
        Guid questionId,
        decimal number)
    {
        return Result.Success(
            new AttemptAnswer(questionId)
            {
                NumberAnswer = number,
            });
    }

    public static Result<AttemptAnswer> CreateMatching(
        Guid questionId,
        IEnumerable<AttemptMatchingPair> pairs)
    {
        var pairList = pairs.ToList();

        if (pairList.Count == 0)
        {
            return Result.Failure<AttemptAnswer>("attempt.answer.pair_required");
        }

        if (pairList.GroupBy(x => x.LeftOptionId).Any(x => x.Count() > 1))
        {
            return Result.Failure<AttemptAnswer>("attempt.answer.duplicate_left_item");
        }

        if (pairList.GroupBy(x => x.RightOptionId).Any(x => x.Count() > 1))
        {
            return Result.Failure<AttemptAnswer>("attempt.answer.duplicate_right_item");
        }

        var answer = new AttemptAnswer(questionId);

        answer._matchingPairs.AddRange(pairList);

        return Result.Success(answer);
    }

    public object ToEvaluationValue()
    {
        if (_selectedOptionIds.Count > 0)
        {
            return _selectedOptionIds.ToList();
        }

        if (TextAnswer is not null)
        {
            return TextAnswer;
        }

        if (NumberAnswer.HasValue)
        {
            return NumberAnswer.Value;
        }

        return _matchingPairs.ToDictionary(x => x.LeftOptionId, x => x.RightOptionId);
    }
}
