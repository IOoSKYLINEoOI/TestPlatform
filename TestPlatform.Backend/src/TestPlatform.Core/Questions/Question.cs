using CSharpFunctionalExtensions;
using TestPlatform.Core.Questions.AnswerDefinition.Abstractions;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Core.Questions;

public class Question
{
    private const int MaxLengthText = 200;

    private readonly List<Tag> _tags = new();

    private Question() { }

    private Question(
        Guid id,
        string text,
        QuestionAnswerDefinition answerDefinition)
    {
        Id = id;
        Text = text;
        AnswerDefinition = answerDefinition;
    }

    public Guid Id { get; }

    public string Text { get; private set; }

    public QuestionType QuestionType => AnswerDefinition.Type;

    public Guid? ImageId { get; private set; }

    public QuestionAnswerDefinition AnswerDefinition { get; private set; }

    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    public static Result<Question> Create(
        string text,
        QuestionAnswerDefinition answerDefinition)
    {
        var validation = Validate(text);
        if (validation.IsFailure)
            return Result.Failure<Question>(validation.Error);

        return Result.Success(new Question(Guid.NewGuid(), text, answerDefinition));
    }

    public Result Update(string text, QuestionAnswerDefinition answerDefinition)
    {
        var validation = Validate(text);
        if (validation.IsFailure)
            return validation;

        Text = text;
        AnswerDefinition = answerDefinition;

        return Result.Success();
    }

    public Result ChangeImage(Guid? fileAssetId)
    {
        ImageId = fileAssetId;

        return Result.Success();
    }

    public Result AddTag(Tag tag)
    {
        if (_tags.Any(t => t.Id == tag.Id))
            return Result.Failure("Такой тег уже добавлен.");

        _tags.Add(tag);
        return Result.Success();
    }

    public Result AddTags(IEnumerable<Tag> tags)
    {
        foreach (var tag in tags.DistinctBy(t => t.Id))
        {
            var result = AddTag(tag);
            if (result.IsFailure)
                return result;
        }

        return Result.Success();
    }

    public void ReplaceTags(IEnumerable<Tag> tags)
    {
        _tags.Clear();

        foreach (var tag in tags.DistinctBy(t => t.Id))
        {
            _tags.Add(tag);
        }
    }

    private static Result Validate(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > MaxLengthText)
            return Result.Failure($"'{nameof(text)}' не может быть пустым или длиннее {MaxLengthText} символов.");

        return Result.Success();
    }
}