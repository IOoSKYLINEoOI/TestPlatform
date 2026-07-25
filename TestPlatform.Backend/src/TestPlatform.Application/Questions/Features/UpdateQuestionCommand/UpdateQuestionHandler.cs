using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Files;
using TestPlatform.Application.Questions.Factories;
using TestPlatform.Application.Questions.Services;
using TestPlatform.Application.Tags;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Questions.DTOs;
using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Questions.Features.UpdateQuestionCommand;

public record UpdateQuestionCommand(Guid Id, QuestionRequest Request) : ICommand;

public class UpdateQuestionHandler : ICommandHandler<UpdateQuestionCommand>
{
    private readonly IAccessService<Question> _questionAccessService;
    private readonly ITagsReadDbContext _tagsReadDbContext;
    private readonly QuestionFileAttachmentService _fileAttachmentService;
    private readonly IFileAssetService _fileAssetService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateQuestionHandler(
        IAccessService<Question> questionAccessService,
        ITagsReadDbContext tagsReadDbContext,
        QuestionFileAttachmentService fileAttachmentService,
        IFileAssetService fileAssetService,
        ICurrentUserAccessor currentUserAccessor,
        IUnitOfWork unitOfWork)
    {
        _questionAccessService = questionAccessService;
        _tagsReadDbContext = tagsReadDbContext;
        _fileAttachmentService = fileAttachmentService;
        _fileAssetService = fileAssetService;
        _currentUserAccessor = currentUserAccessor;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateQuestionCommand command, CancellationToken cancellationToken)
    {
        var accessResult = await _questionAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure(accessResult.Error);
        }

        var question = accessResult.Value;
        var previousFileIds = question.GetReferencedFileIds();

        var tagIds = command.Request.TagIds.Distinct().ToList();

        var tags = await _tagsReadDbContext.ReadTags
            .Where(t => tagIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        var missingTags = tagIds.Except(tags.Select(t => t.Id)).ToList();

        if (missingTags.Count != 0)
        {
            return Result.Failure("question.tags_not_found");
        }

        var definitionResult = QuestionAnswerDefinitionFactory.Create(command.Request);
        if (definitionResult.IsFailure)
        {
            return Result.Failure(definitionResult.Error);
        }

        var contentResult = QuestionContent.Create(command.Request.Text, command.Request.Explanation);
        if (contentResult.IsFailure)
        {
            return Result.Failure(contentResult.Error);
        }

        var updatedQuestionResult = question.UpdateContent(
            contentResult.Value,
            definitionResult.Value);
        if (updatedQuestionResult.IsFailure)
        {
            return Result.Failure(updatedQuestionResult.Error);
        }

        var oldImageId = question.ImageId;

        if (command.Request.ImageId != oldImageId)
        {
            var changeImageResult = question.ReplaceImage(command.Request.ImageId);

            if (changeImageResult.IsFailure)
            {
                return Result.Failure(changeImageResult.Error);
            }
        }

        var replaceTagsResult = question.ReplaceTags(tags);
        if (replaceTagsResult.IsFailure)
        {
            return Result.Failure(replaceTagsResult.Error);
        }

        var currentUser = _currentUserAccessor.User;
        if (currentUser is null)
        {
            return Result.Failure(ErrorCodes.Unauthorized);
        }

        var attachResult = await _fileAttachmentService.AttachNewFilesAsync(
            question,
            previousFileIds,
            currentUser.Id,
            cancellationToken);
        if (attachResult.IsFailure)
        {
            return Result.Failure(attachResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var currentFileIds = question.GetReferencedFileIds();
        foreach (var fileId in previousFileIds.Except(currentFileIds))
        {
            await _fileAssetService.ReleaseIfUnreferencedAsync(fileId, cancellationToken);
        }

        return Result.Success();
    }
}
