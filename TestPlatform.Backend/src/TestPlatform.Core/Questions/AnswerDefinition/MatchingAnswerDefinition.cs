using System.Diagnostics;
using CSharpFunctionalExtensions;
using TestPlatform.Core.Questions.AnswerDefinition.Abstractions;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Core.Questions.AnswerDefinition;

public class MatchingAnswerDefinition : TypedQuestionAnswerDefinition<Dictionary<Guid, Guid>>
{
    private readonly List<MatchingPair> _pairs = new();
    private readonly List<MatchingItem> _leftItems = new();
    private readonly List<MatchingItem> _rightItems = new();

    private MatchingAnswerDefinition(
        EvaluationMode mode,
        IEnumerable<MatchingItem> leftItems,
        IEnumerable<MatchingItem> rightItems,
        IEnumerable<MatchingPair> pairs)
    {
        Mode = mode;

        _leftItems.AddRange(leftItems);
        _rightItems.AddRange(rightItems);

        _pairs.AddRange(pairs);
    }

    public override QuestionType Type => QuestionType.Matching;

    public EvaluationMode Mode { get; }

    public IReadOnlyCollection<MatchingItem> LeftItems => _leftItems.AsReadOnly();

    public IReadOnlyCollection<MatchingItem> RightItems => _rightItems.AsReadOnly();

    public IReadOnlyCollection<MatchingPair> Pairs => _pairs.AsReadOnly();

    public static Result<MatchingAnswerDefinition> Create(
        EvaluationMode mode,
        IEnumerable<MatchingItem> leftItems,
        IEnumerable<MatchingItem> rightItems,
        IEnumerable<MatchingPair> pairs)
    {
        var leftList = leftItems.ToList();
        var rightList = rightItems.ToList();
        var pairList = pairs.ToList();

        if (!pairList.Any())
            return Result.Failure<MatchingAnswerDefinition>("Должна быть хотя бы одна пара.");

        if (!leftList.Any())
            return Result.Failure<MatchingAnswerDefinition>("Должен быть хотя бы один элемент слева.");

        if (!rightList.Any())
            return Result.Failure<MatchingAnswerDefinition>("Должен быть хотя бы один элемент справа.");

        var leftIds = leftList.Select(x => x.Id).ToHashSet();
        var rightIds = rightList.Select(x => x.Id).ToHashSet();

        var invalidPairs = pairList
            .Where(p => !leftIds.Contains(p.LeftId) || !rightIds.Contains(p.RightId))
            .ToList();

        if (leftList.Count != rightList.Count)
            return Result.Failure<MatchingAnswerDefinition>("Количество элементов слева и справа должно совпадать.");

        if (pairList.GroupBy(x => x.LeftId).Any(g => g.Count() > 1))
            return Result.Failure<MatchingAnswerDefinition>("Левый элемент не может входить в несколько пар.");

        if (pairList.GroupBy(x => x.RightId).Any(g => g.Count() > 1))
            return Result.Failure<MatchingAnswerDefinition>("Правый элемент не может входить в несколько пар.");

        if (pairList.Count != leftList.Count)
            return Result.Failure<MatchingAnswerDefinition>("Каждый элемент должен участвовать в одной паре.");

        if (invalidPairs.Any())
            return Result.Failure<MatchingAnswerDefinition>("Пары содержат несуществующие элементы.");

        return Result.Success(
            new MatchingAnswerDefinition(mode, leftList, rightList, pairList));
    }

    public override decimal GetScore(Dictionary<Guid, Guid> userPairs)
    {
        if (userPairs.Count == 0)
            return 0m;

        var correctPairs = _pairs.ToDictionary(x => x.LeftId, x => x.RightId);

        var matchedUserPairs = userPairs
            .Where(x => correctPairs.ContainsKey(x.Key))
            .ToDictionary(x => x.Key, x => x.Value);

        var correctCount = matchedUserPairs.Count(x =>
            correctPairs[x.Key] == x.Value);

        return Mode switch
        {
            EvaluationMode.Strict =>
                matchedUserPairs.Count == correctPairs.Count &&
                correctCount == correctPairs.Count
                    ? 1m
                    : 0m,

            EvaluationMode.Partial =>
                correctPairs.Count == 0
                    ? 0m
                    : (decimal)correctCount / correctPairs.Count,

            _ => throw new UnreachableException()
        };
    }
}