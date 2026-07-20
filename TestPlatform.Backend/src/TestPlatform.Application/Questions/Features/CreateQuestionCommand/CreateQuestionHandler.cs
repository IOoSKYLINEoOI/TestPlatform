using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;
using TestPlatform.Application.Files;
using TestPlatform.Application.Questions.Factories;
using TestPlatform.Application.Questions.Tags;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Core.Questions;


namespace TestPlatform.Application.Questions.Features.CreateQuestionCommand;

public record CreateQuestionCommand(QuestionRequest Request) : ICommand;

public class CreateQuestionHandler : ICommandHandler<CreateQuestionCommand, Guid>
{
    private readonly IQuestionsRepository _questionsRepository;
    private readonly IFileAssetService _fileAssetService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ITagsReadDbContext _tagsReadDbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateQuestionHandler> _logger;

    public CreateQuestionHandler(
        IQuestionsRepository questionsRepository,
        IFileAssetService fileAssetService,
        ICurrentUserAccessor currentUserAccessor,
        ITagsReadDbContext tagsReadDbContext,
        IUnitOfWork unitOfWork,
        ILogger<CreateQuestionHandler> logger)
    {
        _questionsRepository = questionsRepository;
        _fileAssetService = fileAssetService;
        _currentUserAccessor = currentUserAccessor;
        _tagsReadDbContext = tagsReadDbContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateQuestionCommand command, CancellationToken cancellationToken)
    {
        var definitionResult = QuestionAnswerDefinitionFactory.Create(command.Request);

        if (definitionResult.IsFailure)
            return Result.Failure<Guid>(definitionResult.Error);

        var questionResult = Question.Create(
            command.Request.Text,
            definitionResult.Value);

        if (questionResult.IsFailure)
            return Result.Failure<Guid>(questionResult.Error);

        var question = questionResult.Value;

        var tagIds = command.Request.TagIds.Distinct().ToList();

        var tags = await _tagsReadDbContext.ReadTags
            .Where(t => tagIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        var missingTags = tagIds.Except(tags.Select(t => t.Id)).ToList();

        if (missingTags.Count != 0)
            return Result.Failure<Guid>("One or more tags do not exist.");

        var tagResult = question.AddTags(tags);

        if (tagResult.IsFailure)
            return Result.Failure<Guid>(tagResult.Error);

        if (command.Request.ImageId.HasValue)
        {
            var currentUser = _currentUserAccessor.User;
            if (currentUser is null)
                return Result.Failure<Guid>("unauthorized");

            var attachResult = await _fileAssetService.AttachAsync(
                command.Request.ImageId.Value,
                currentUser.Id,
                cancellationToken);

            if (attachResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to attach image {ImageId}: {Error}",
                    command.Request.ImageId,
                    attachResult.Error);

                return Result.Failure<Guid>(attachResult.Error);
            }

            question.ChangeImage(command.Request.ImageId);
        }

        await _questionsRepository.AddAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogResult("Create Question", question.Id, Result.Success());

        return Result.Success(question.Id);
    }
}
