using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Files;
using TestPlatform.Application.Questions.Factories;
using TestPlatform.Application.Tags;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Questions.DTOs;

namespace TestPlatform.Application.Questions.Features.UpdateQuestionCommand;

public record UpdateQuestionCommand(Guid Id, QuestionRequest Request) : ICommand;

public class UpdateQuestionHandler : ICommandHandler<UpdateQuestionCommand>
{
    private readonly IQuestionsRepository _questionsRepository;
    private readonly ITagsReadDbContext _tagsReadDbContext;
    private readonly IFileAssetService _fileAssetService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateQuestionHandler(
        IQuestionsRepository questionsRepository,
        ITagsReadDbContext tagsReadDbContext,
        IFileAssetService fileAssetService,
        ICurrentUserAccessor currentUserAccessor,
        IUnitOfWork unitOfWork)
    {
        _questionsRepository = questionsRepository;
        _tagsReadDbContext = tagsReadDbContext;
        _fileAssetService = fileAssetService;
        _currentUserAccessor = currentUserAccessor;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateQuestionCommand command, CancellationToken cancellationToken)
    {
        var question = await _questionsRepository.GetByIdAsync(command.Id, cancellationToken);
        if (question is null)
            return Result.Failure(ErrorCodes.QuestionNotFound);

        var tagIds = command.Request.TagIds.Distinct().ToList();

        var tags = await _tagsReadDbContext.ReadTags
            .Where(t => tagIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        var missingTags = tagIds.Except(tags.Select(t => t.Id)).ToList();

        if (missingTags.Count != 0)
            return Result.Failure("One or more tags do not exist.");

        question.ReplaceTags(tags);

        var definitionResult = QuestionAnswerDefinitionFactory.Create(command.Request);
        if (definitionResult.IsFailure)
            return Result.Failure(definitionResult.Error);

        var updatedQuestionResult = question.Update(command.Request.Text, definitionResult.Value);
        if (updatedQuestionResult.IsFailure)
            return Result.Failure(updatedQuestionResult.Error);

        var oldImageId = question.ImageId;

        if (command.Request.ImageId != oldImageId)
        {
            if (command.Request.ImageId.HasValue)
            {
                var currentUser = _currentUserAccessor.User;
                if (currentUser is null)
                    return Result.Failure("unauthorized");

                var attachResult = await _fileAssetService.AttachAsync(
                    command.Request.ImageId.Value,
                    currentUser.Id,
                    cancellationToken);

                if (attachResult.IsFailure)
                    return Result.Failure(attachResult.Error);
            }

            var changeImageResult = question.ChangeImage(command.Request.ImageId);

            if (changeImageResult.IsFailure)
                return Result.Failure(changeImageResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
