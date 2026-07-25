using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Extensions;

namespace TestPlatform.Application.Tags.Features.DeleteTagCommand;

public record DeleteTagCommand(Guid Id) : ICommand;

public class DeleteTagHandler : ICommandHandler<DeleteTagCommand>
{
    private readonly ITagsRepository _tagsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteTagHandler> _logger;

    public DeleteTagHandler(
        ITagsRepository tagsRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteTagHandler> logger)
    {
        _tagsRepository = tagsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTagCommand command, CancellationToken cancellationToken)
    {
        var tag = await _tagsRepository.GetByIdAsync(command.Id, cancellationToken);

        if (tag is null)
        {
            return Result.Failure(ErrorCodes.TagNotFound);
        }

        if (await _tagsRepository.GetUsageCountAsync(tag.Id, cancellationToken) > 0)
        {
            return Result.Failure(ErrorCodes.TagInUse);
        }

        _tagsRepository.Delete(tag);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogResult("Delete Tag", command.Id, Result.Success());

        return Result.Success();
    }
}
