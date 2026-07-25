using CSharpFunctionalExtensions;
using TestPlatform.Core.Questions.AnswerDefinition.Abstractions;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Core.Questions;

public class Question
{
    private readonly List<Tag> _tags = new();

    private Question()
    {
    }

    private Question(Guid id, QuestionContent content, QuestionAnswerDefinition answerDefinition, Guid createdByUserId)
    {
        Id = id;
        Content = content;
        AnswerDefinition = answerDefinition;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        Status = QuestionStatus.Draft;
    }

    public Guid Id { get; }

    public QuestionContent Content { get; private set; } = null!;

    public string Text => Content.Text;

    public string? Explanation => Content.Explanation;

    public QuestionType QuestionType => AnswerDefinition.Type;

    public Guid? ImageId { get; private set; }

    public QuestionAnswerDefinition AnswerDefinition { get; private set; } = null!;

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public QuestionStatus Status { get; private set; }

    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    public static Result<Question> Create(QuestionContent content, QuestionAnswerDefinition answerDefinition, Guid createdByUserId)
    {
        var validation = Validate(content, answerDefinition, createdByUserId);
        if (validation.IsFailure)
        {
            return Result.Failure<Question>(validation.Error);
        }

        return Result.Success(new Question(Guid.NewGuid(), content, answerDefinition, createdByUserId));
    }

    public Result UpdateContent(QuestionContent content, QuestionAnswerDefinition answerDefinition)
    {
        if (Status != QuestionStatus.Draft)
        {
            return Result.Failure("question.not_editable");
        }

        var validation = Validate(content, answerDefinition, CreatedByUserId);
        if (validation.IsFailure)
        {
            return validation;
        }

        Content = content;
        AnswerDefinition = answerDefinition;
        Touch();
        return Result.Success();
    }

    public Result<Guid?> ReplaceImage(Guid? fileAssetId)
    {
        if (Status != QuestionStatus.Draft)
        {
            return Result.Failure<Guid?>("question.not_editable");
        }

        var previousImageId = ImageId;
        ImageId = fileAssetId;
        Touch();
        return Result.Success(previousImageId);
    }

    public Result ReplaceTags(IEnumerable<Tag> tags)
    {
        if (Status != QuestionStatus.Draft)
        {
            return Result.Failure("question.not_editable");
        }

        _tags.Clear();
        _tags.AddRange(tags.DistinctBy(tag => tag.Id));
        Touch();
        return Result.Success();
    }

    public Result Publish()
    {
        if (Status != QuestionStatus.Draft)
        {
            return Result.Failure("question.invalid_status_transition");
        }

        var validation = ValidateForPublication();
        if (validation.IsFailure)
        {
            return validation;
        }

        Status = QuestionStatus.Published;
        Touch();
        return Result.Success();
    }

    public Result Archive()
    {
        if (Status == QuestionStatus.Archived)
        {
            return Result.Success();
        }

        Status = QuestionStatus.Archived;
        Touch();
        return Result.Success();
    }

    public Result<Question> CloneAsDraft(Guid createdByUserId)
    {
        if (Status != QuestionStatus.Published)
        {
            return Result.Failure<Question>("question.clone_requires_published");
        }

        var content = QuestionContent.Create(Text, Explanation);
        if (content.IsFailure)
        {
            return Result.Failure<Question>(content.Error);
        }

        var clone = Create(content.Value, AnswerDefinition.Copy(), createdByUserId);
        if (clone.IsFailure)
        {
            return clone;
        }

        clone.Value._tags.AddRange(_tags);
        clone.Value.ImageId = ImageId;
        return clone;
    }

    public Result ValidateForPublication()
    {
        return Validate(Content, AnswerDefinition, CreatedByUserId);
    }

    public IReadOnlyCollection<Guid> GetReferencedFileIds()
    {
        var fileIds = new List<Guid>();

        if (ImageId.HasValue)
        {
            fileIds.Add(ImageId.Value);
        }

        switch (AnswerDefinition)
        {
            case AnswerDefinition.ChoiceAnswerDefinition choice:
                fileIds.AddRange(choice.Options.Where(option => option.ImageId.HasValue).Select(option => option.ImageId!.Value));
                break;
            case AnswerDefinition.MatchingAnswerDefinition matching:
                fileIds.AddRange(matching.LeftItems.Where(item => item.ImageId.HasValue).Select(item => item.ImageId!.Value));
                fileIds.AddRange(matching.RightItems.Where(item => item.ImageId.HasValue).Select(item => item.ImageId!.Value));
                break;
        }

        return fileIds.Distinct().ToList();
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;

    private static Result Validate(QuestionContent content, QuestionAnswerDefinition answerDefinition, Guid createdByUserId)
    {
        if (content is null)
        {
            return Result.Failure("question.content_required");
        }

        if (answerDefinition is null)
        {
            return Result.Failure("question.answer_definition_required");
        }

        if (createdByUserId == Guid.Empty)
        {
            return Result.Failure("question.author_required");
        }

        return Result.Success();
    }
}
