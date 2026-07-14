using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Abstractions.Enums;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Questions.Factories;
using TestPlatform.Application.Tags;
using TestPlatform.Contracts.Questions.DTOs;

namespace TestPlatform.Application.Questions.Features.UpdateQuestionCommand;

public record UpdateQuestionCommand(Guid Id, QuestionRequest Request) : ICommand;

public class UpdateQuestionHandler : ICommandHandler<UpdateQuestionCommand>
{
    private readonly IQuestionsRepository _questionsRepository;
    private readonly ITagsReadDbContext _tagsReadDbContext;
    private readonly IImageStorageService _imageStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateQuestionHandler(
        IQuestionsRepository questionsRepository,
        ITagsReadDbContext tagsReadDbContext,
        IImageStorageService imageStorageService,
        IUnitOfWork unitOfWork)
    {
        _questionsRepository = questionsRepository;
        _tagsReadDbContext = tagsReadDbContext;
        _imageStorageService = imageStorageService;
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

        var oldImageName = question.ImageName;

        if (command.Request.ImageName != oldImageName)
        {
            if (!string.IsNullOrWhiteSpace(command.Request.ImageName))
            {
                var moveResult = await _imageStorageService.MoveToPermanent(
                    command.Request.ImageName,
                    ImageFolder.QUESTIONS);

                if (moveResult.IsFailure)
                    return Result.Failure(moveResult.Error);
            }

            var changeImageResult = question.ChangeImage(command.Request.ImageName);

            if (changeImageResult.IsFailure)
                return Result.Failure(changeImageResult.Error);

            if (!string.IsNullOrWhiteSpace(oldImageName))
                await _imageStorageService.DeletePermanentAsync(ImageFolder.QUESTIONS, oldImageName);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
