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

        if (pairList.Count == 0)
        {
            return Result.Failure<MatchingAnswerDefinition>("question.answer.matching_pair_required");
        }

        if (leftList.Count == 0)
        {
            return Result.Failure<MatchingAnswerDefinition>("question.answer.left_item_required");
        }

        if (rightList.Count == 0)
        {
            return Result.Failure<MatchingAnswerDefinition>("question.answer.right_item_required");
        }

        var leftIds = leftList.Select(x => x.Id).ToHashSet();
        var rightIds = rightList.Select(x => x.Id).ToHashSet();

        var invalidPairs = pairList
            .Where(p => !leftIds.Contains(p.LeftId) || !rightIds.Contains(p.RightId))
            .ToList();

        if (leftList.Count != rightList.Count)
        {
            return Result.Failure<MatchingAnswerDefinition>("question.answer.matching_item_count_mismatch");
        }

        if (leftList.Select(item => item.Id).Distinct().Count() != leftList.Count ||
            rightList.Select(item => item.Id).Distinct().Count() != rightList.Count)
        {
            return Result.Failure<MatchingAnswerDefinition>("question.answer.duplicate_matching_item_ids");
        }

        if (pairList.GroupBy(x => x.LeftId).Any(g => g.Count() > 1))
        {
            return Result.Failure<MatchingAnswerDefinition>("question.answer.duplicate_left_item");
        }

        if (pairList.GroupBy(x => x.RightId).Any(g => g.Count() > 1))
        {
            return Result.Failure<MatchingAnswerDefinition>("question.answer.duplicate_right_item");
        }

        if (pairList.Count != leftList.Count)
        {
            return Result.Failure<MatchingAnswerDefinition>("question.answer.unmatched_item");
        }

        if (invalidPairs.Count > 0)
        {
            return Result.Failure<MatchingAnswerDefinition>("question.answer.unknown_matching_item");
        }

        return Result.Success(
            new MatchingAnswerDefinition(mode, leftList, rightList, pairList));
    }

    public override QuestionAnswerDefinition Copy()
    {
        var leftItems = _leftItems
            .Select(item => MatchingItem.Create(item.Text, item.ImageId).Value)
            .ToList();
        var rightItems = _rightItems
            .Select(item => MatchingItem.Create(item.Text, item.ImageId).Value)
            .ToList();

        var leftIds = _leftItems
            .Zip(leftItems)
            .ToDictionary(pair => pair.First.Id, pair => pair.Second.Id);
        var rightIds = _rightItems
            .Zip(rightItems)
            .ToDictionary(pair => pair.First.Id, pair => pair.Second.Id);
        var pairs = _pairs
            .Select(pair => new MatchingPair(leftIds[pair.LeftId], rightIds[pair.RightId]))
            .ToList();

        return new MatchingAnswerDefinition(Mode, leftItems, rightItems, pairs);
    }

    public override decimal GetScore(Dictionary<Guid, Guid> userPairs)
    {
        if (userPairs.Count == 0)
        {
            return 0m;
        }

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
