using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;
using TestPlatform.Application.Questions.Factories;
using TestPlatform.Application.Questions.Services;
using TestPlatform.Application.Tags;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Core.Questions;


namespace TestPlatform.Application.Questions.Features.CreateQuestionCommand;

public record CreateQuestionCommand(QuestionRequest Request) : ICommand;

public class CreateQuestionHandler : ICommandHandler<CreateQuestionCommand, Guid>
{
    private readonly IQuestionsRepository _questionsRepository;
    private readonly QuestionFileAttachmentService _fileAttachmentService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ITagsReadDbContext _tagsReadDbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateQuestionHandler> _logger;

    public CreateQuestionHandler(
        IQuestionsRepository questionsRepository,
        QuestionFileAttachmentService fileAttachmentService,
        ICurrentUserAccessor currentUserAccessor,
        ITagsReadDbContext tagsReadDbContext,
        IUnitOfWork unitOfWork,
        ILogger<CreateQuestionHandler> logger)
    {
        _questionsRepository = questionsRepository;
        _fileAttachmentService = fileAttachmentService;
        _currentUserAccessor = currentUserAccessor;
        _tagsReadDbContext = tagsReadDbContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateQuestionCommand command, CancellationToken cancellationToken)
    {
        var definitionResult = QuestionAnswerDefinitionFactory.Create(command.Request);

        if (definitionResult.IsFailure)
        {
            return Result.Failure<Guid>(definitionResult.Error);
        }

        var contentResult = QuestionContent.Create(command.Request.Text, command.Request.Explanation);
        if (contentResult.IsFailure)
        {
            return Result.Failure<Guid>(contentResult.Error);
        }

        var currentUser = _currentUserAccessor.User;
        if (currentUser is null)
        {
            return Result.Failure<Guid>("unauthorized");
        }

        var questionResult = Question.Create(
            contentResult.Value,
            definitionResult.Value,
            currentUser.Id);

        if (questionResult.IsFailure)
        {
            return Result.Failure<Guid>(questionResult.Error);
        }

        var question = questionResult.Value;

        var tagIds = command.Request.TagIds.Distinct().ToList();

        var tags = await _tagsReadDbContext.ReadTags
            .Where(t => tagIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        var missingTags = tagIds.Except(tags.Select(t => t.Id)).ToList();

        if (missingTags.Count != 0)
        {
            return Result.Failure<Guid>("question.tags_not_found");
        }

        var tagResult = question.ReplaceTags(tags);

        if (tagResult.IsFailure)
        {
            return Result.Failure<Guid>(tagResult.Error);
        }

        if (command.Request.ImageId.HasValue)
        {
            var imageResult = question.ReplaceImage(command.Request.ImageId);
            if (imageResult.IsFailure)
            {
                return Result.Failure<Guid>(imageResult.Error);
            }
        }

        var attachResult = await _fileAttachmentService.AttachNewFilesAsync(
            question,
            [],
            currentUser.Id,
            cancellationToken);
        if (attachResult.IsFailure)
        {
            return Result.Failure<Guid>(attachResult.Error);
        }

        await _questionsRepository.AddAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogResult("Create Question", question.Id, Result.Success());

        return Result.Success(question.Id);
    }
}
