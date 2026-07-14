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
            return Result.Failure<ChoiceAnswerDefinition>("Для SingleChoice доступен только строгий режим оценки.");

        var optionList = options.ToList();
        if (optionList.Count == 0)
            return Result.Failure<ChoiceAnswerDefinition>("Должен быть хотя бы один вариант ответа.");

        if (optionList.Count > MaxOptions)
            return Result.Failure<ChoiceAnswerDefinition>($"Максимум {MaxOptions} вариантов ответа.");

        var correctCount = optionList.Count(x => x.IsCorrect);

        if (mode == ChoiceMode.Single && correctCount != 1)
            return Result.Failure<ChoiceAnswerDefinition>("SingleChoice должен иметь ровно один правильный ответ.");

        if (mode == ChoiceMode.Multiple && correctCount < 1)
            return Result.Failure<ChoiceAnswerDefinition>("MultipleChoice должен иметь хотя бы один правильный ответ.");

        return Result.Success(new ChoiceAnswerDefinition(mode, evaluationMode, optionList));
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
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static decimal CalculatePartial(HashSet<Guid> correctIds, HashSet<Guid> selectedIds)
    {
        if (correctIds.Count == 0)
            return 0m;

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