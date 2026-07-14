using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Abstractions.Enums;
using TestPlatform.Application.Extensions;
using TestPlatform.Application.Questions.Factories;
using TestPlatform.Application.Tags;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Core.Questions;


namespace TestPlatform.Application.Questions.Features.CreateQuestionCommand;

public record CreateQuestionCommand(QuestionRequest Request) : ICommand;

public class CreateQuestionHandler : ICommandHandler<CreateQuestionCommand, Guid>
{
    private readonly IQuestionsRepository _questionsRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly ITagsReadDbContext _tagsReadDbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateQuestionHandler> _logger;

    public CreateQuestionHandler(
        IQuestionsRepository questionsRepository,
        IImageStorageService imageStorageService,
        ITagsReadDbContext tagsReadDbContext,
        IUnitOfWork unitOfWork,
        ILogger<CreateQuestionHandler> logger)
    {
        _questionsRepository = questionsRepository;
        _imageStorageService = imageStorageService;
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

        if (!string.IsNullOrWhiteSpace(command.Request.ImageName))
        {
            var moveResult = await _imageStorageService.MoveToPermanent(
                command.Request.ImageName,
                ImageFolder.QUESTIONS);

            if (moveResult.IsFailure)
            {
                _logger.LogWarning(
                    "Failed to move image {ImageName} to permanent storage: {Error}",
                    command.Request.ImageName,
                    moveResult.Error);

                return Result.Failure<Guid>(moveResult.Error);
            }

            question.ChangeImage(command.Request.ImageName);
        }

        await _questionsRepository.AddAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogResult("Create Question", question.Id, Result.Success());

        return Result.Success(question.Id);
    }
}