/*
using CSharpFunctionalExtensions;
using TestPlatform.Core.Questions;
using TestPlatform.Core.Questions.AnswerDefinition;

namespace TestPlatform.Core.Attempts.Snapshot;

public class AttemptQuestionSnapshot
{
    private readonly List<AttemptOptionSnapshot> _options;
    private readonly List<MatchingPair> _correctPairs;

    private AttemptQuestionSnapshot(
        Guid questionId,
        QuestionType questionType,
        string text,
        decimal score,
        int order,
        decimal? correctNumber,
        string? correctText,
        IEnumerable<AttemptOptionSnapshot> options,
        IEnumerable<MatchingPair> correctPairs)
    {
        QuestionId = questionId;
        QuestionType = questionType;
        Text = text;
        Score = score;
        Order = order;
        CorrectNumber = correctNumber;
        CorrectText = correctText;

        _options = options.ToList();
        _correctPairs = correctPairs.ToList();
    }

    public Guid QuestionId { get; }

    public QuestionType QuestionType { get; }

    public string Text { get; }

    public decimal Score { get; }

    public int Order { get; }

    public decimal? CorrectNumber { get; }

    public string? CorrectText { get; }

    public IReadOnlyCollection<AttemptOptionSnapshot> Options => _options;

    public List<MatchingPair> CorrectPairs => _correctPairs;


    public static Result<AttemptQuestionSnapshot> Create(
        Guid questionId,
        QuestionType questionType,
        string text,
        decimal score,
        int order,
        decimal? correctNumber,
        string? correctText,
        IEnumerable<AttemptOptionSnapshot> options,
        IEnumerable<MatchingPair> correctPairs)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Result.Failure<AttemptQuestionSnapshot>("Текст вопроса обязателен");

        if (questionType == QuestionType.SingleChoice || questionType == QuestionType.MultipleChoiceStrict)
        {
            var optionList = options.ToList();

            if (!optionList.Any())
                return Result.Failure<AttemptQuestionSnapshot>("Должен быть хотя бы один вариант");

            if (!optionList.Any(x => x.IsCorrect))
                return Result.Failure<AttemptQuestionSnapshot>("Должен быть хотя бы один правильный ответ");
        }

        if (questionType == QuestionType.Number)
        {
            if (correctNumber is null)
                return Result.Failure<AttemptQuestionSnapshot>("Должен быть правильный ответ");
        }

        if (questionType == QuestionType.Text)
        {
            if (correctText is null)
                return Result.Failure<AttemptQuestionSnapshot>("Должен быть правильный ответ");
        }

        /*if (questionType == QuestionType.Matching)
        {
            var correctPairsList = correctPairs.ToList();

            if (!correctPairs.Any())
                return Result.Failure<AttemptQuestionSnapshot>("Должен быть хотя бы один вариант");
        }#1#

        return Result.Success(
            new AttemptQuestionSnapshot(
                questionId,
                questionType,
                text,
                score,
                order,
                correctNumber,
                correctText,
                options,
                correctPairs));
    }

    public Result<decimal> CalculateScore(AttemptAnswer answer)
    {
        if (answer.QuestionId != QuestionId)
            return Result.Failure<decimal>("Ответ не принадлежит вопросу");

        return QuestionType switch
        {
            QuestionType.SingleChoice => CalculateSingleChoice(answer),
            QuestionType.MultipleChoiceStrict => CalculateMultipleChoice(answer),
            QuestionType.Text => CalculateText(answer),
            QuestionType.Number => CalculateNumber(answer),
            QuestionType.Matching => CalculateMatching(answer),
            _ => Result.Failure<decimal>($"Тип {QuestionType} не поддерживается")
        };
    }

    private Result<decimal> CalculateSingleChoice(
        AttemptAnswer answer)
    {
        if (answer.SelectedOptionIds.Count != 1)
            return Result.Success(0m);

        var correctAnswer = Options.FirstOrDefault(x => x.IsCorrect);

        if (correctAnswer is null)
            return Result.Failure<decimal>("У вопроса отсутствует правильный ответ");

        var selectedOptionId = answer.SelectedOptionIds.First();

        return Result.Success(
            correctAnswer.Id == selectedOptionId
                ? Score
                : 0m);
    }

    private Result<decimal> CalculateMultipleChoice(
        AttemptAnswer answer)
    {
        var selected = answer.SelectedOptionIds.ToHashSet();

        var correct = _options
            .Where(x => x.IsCorrect)
            .Select(x => x.Id)
            .ToHashSet();

        return Result.Success(
            selected.SetEquals(correct)
                ? Score
                : 0m);
    }

    private Result<decimal> CalculateText(
        AttemptAnswer answer)
    {
        if (string.IsNullOrWhiteSpace(answer.TextAnswer))
            return Result.Success(0m);

        return Result.Success(
            string.Equals(
                answer.TextAnswer.Trim(),
                _correctText,
                StringComparison.OrdinalIgnoreCase)
                ? Score
                : 0m);
    }

    private Result<decimal> CalculateNumber(
        AttemptAnswer answer)
    {
        if (answer.NumberAnswer is null)
            return Result.Success(0m);

        return Result.Success(
            answer.NumberAnswer == _correctNumber
                ? Score
                : 0m);
    }

    private Result<decimal> CalculateMatching(
        AttemptAnswer answer)
    {
        var selected = answer.MatchingPairs.ToHashSet();
        var correct = _correctPairs.ToHashSet();

        return Result.Success(
            selected.SetEquals(correct)
                ? Score
                : 0m);
    }
}
*/