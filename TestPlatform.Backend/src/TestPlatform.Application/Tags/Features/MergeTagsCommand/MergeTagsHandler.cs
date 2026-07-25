using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;

namespace TestPlatform.Application.Tags.Features.MergeTagsCommand;

public record MergeTagsCommand(Guid SourceTagId, Guid TargetTagId) : ICommand;

public class MergeTagsHandler : ICommandHandler<MergeTagsCommand, int>
{
    private readonly ITagsRepository _tagsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MergeTagsHandler> _logger;

    public MergeTagsHandler(
        ITagsRepository tagsRepository,
        IUnitOfWork unitOfWork,
        ILogger<MergeTagsHandler> logger)
    {
        _tagsRepository = tagsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(MergeTagsCommand command, CancellationToken cancellationToken)
    {
        if (command.SourceTagId == command.TargetTagId)
        {
            return Result.Failure<int>(ErrorCodes.TagMergeSameTarget);
        }

        var sourceTag = await _tagsRepository.GetByIdAsync(command.SourceTagId, cancellationToken);
        var targetTag = await _tagsRepository.GetByIdAsync(command.TargetTagId, cancellationToken);

        if (sourceTag is null || targetTag is null)
        {
            return Result.Failure<int>(ErrorCodes.TagNotFound);
        }

        var affectedQuestions = await _tagsRepository.MergeAsync(sourceTag, targetTag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Merged tag {SourceTagId} into {TargetTagId}; updated {AffectedQuestionCount} questions",
            sourceTag.Id,
            targetTag.Id,
            affectedQuestions);

        return Result.Success(affectedQuestions);
    }
}
