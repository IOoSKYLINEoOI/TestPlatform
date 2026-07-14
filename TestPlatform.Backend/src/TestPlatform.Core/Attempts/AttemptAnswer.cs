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

        if (!ids.Any())
            return Result.Failure<AttemptAnswer>("Должен быть выбран хотя бы один вариант.");

        var answer = new AttemptAnswer(questionId);

        answer._selectedOptionIds.AddRange(ids);

        return Result.Success(answer);
    }

    public static Result<AttemptAnswer> CreateText(
        Guid questionId,
        string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Result.Failure<AttemptAnswer>("Текст ответа обязателен.");

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

        if (!pairList.Any())
            return Result.Failure<AttemptAnswer>("Должна быть хотя бы одна пара.");

        var answer = new AttemptAnswer(questionId);

        answer._matchingPairs.AddRange(pairList);

        return Result.Success(answer);
    }
}