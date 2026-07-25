using CSharpFunctionalExtensions;
using TestPlatform.Core.Questions.AnswerDefinition.Abstractions;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Core.Questions.AnswerDefinition;

public class ChoiceAnswerDefinition : TypedQuestionAnswerDefinition<List<Guid>>
{
    private const int MaxOptions = 50;
    private readonly List<AnswerOption> _options = new();

    private ChoiceAnswerDefinition(
        ChoiceMode mode,
        EvaluationMode evaluationMode,
        IEnumerable<AnswerOption> options)
    {
        Mode = mode;
        EvaluationMode = evaluationMode;

        _options.AddRange(options);
    }

    public override QuestionType Type => QuestionType.Choice;

    public ChoiceMode Mode { get; }

    public EvaluationMode EvaluationMode { get; }

    public IReadOnlyCollection<AnswerOption> Options => _options.AsReadOnly();

    public static Result<ChoiceAnswerDefinition> Create(
        ChoiceMode mode,
        EvaluationMode evaluationMode,
        IEnumerable<AnswerOption> options)
    {
        if (mode == ChoiceMode.Single && evaluationMode != EvaluationMode.Strict)
        {
            return Result.Failure<ChoiceAnswerDefinition>("question.answer.single_choice_requires_strict_mode");
        }

        var optionList = options.ToList();
        if (optionList.Count < 2)
        {
            return Result.Failure<ChoiceAnswerDefinition>("question.answer.too_few_options");
        }

        if (optionList.Count > MaxOptions)
        {
            return Result.Failure<ChoiceAnswerDefinition>("question.answer.too_many_options");
        }

        if (optionList.GroupBy(option => option.Text.Trim(), StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
        {
            return Result.Failure<ChoiceAnswerDefinition>("question.answer.duplicate_options");
        }

        var correctCount = optionList.Count(x => x.IsCorrect);

        if (mode == ChoiceMode.Single && correctCount != 1)
        {
            return Result.Failure<ChoiceAnswerDefinition>("question.answer.single_choice_requires_one_correct");
        }

        if (mode == ChoiceMode.Multiple && correctCount < 1)
        {
            return Result.Failure<ChoiceAnswerDefinition>("question.answer.correct_option_required");
        }

        return Result.Success(new ChoiceAnswerDefinition(mode, evaluationMode, optionList));
    }

    public override QuestionAnswerDefinition Copy()
    {
        var options = _options
            .Select(option => AnswerOption.Create(option.Text, option.IsCorrect, option.ImageId).Value)
            .ToList();

        return new ChoiceAnswerDefinition(Mode, EvaluationMode, options);
    }

    public override decimal GetScore(List<Guid> selected)
    {
        var correctIds = _options
            .Where(x => x.IsCorrect)
            .Select(x => x.Id)
            .ToHashSet();

        var selectedIds = selected.ToHashSet();

        return EvaluationMode switch
        {
            EvaluationMode.Strict => correctIds.SetEquals(selectedIds) ? 1m : 0m,
            EvaluationMode.Partial => CalculatePartial(correctIds, selectedIds),
            _ => throw new InvalidOperationException($"Unsupported evaluation mode: {EvaluationMode}.")
        };
    }

    private static decimal CalculatePartial(HashSet<Guid> correctIds, HashSet<Guid> selectedIds)
    {
        if (correctIds.Count == 0)
        {
            return 0m;
        }

        int correctSelected = selectedIds
            .Intersect(correctIds)
            .Count();

        int wrongSelected = selectedIds
            .Except(correctIds)
            .Count();

        decimal score = (decimal)(correctSelected - wrongSelected) / correctIds.Count;

        return Math.Clamp(score, 0m, 1m);
    }
}
