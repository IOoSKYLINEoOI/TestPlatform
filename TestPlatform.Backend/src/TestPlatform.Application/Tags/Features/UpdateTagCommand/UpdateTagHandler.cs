using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Extensions;

namespace TestPlatform.Application.Tags.Features.UpdateTagCommand;

public record UpdateTagCommand(Guid Id, string Name, string Description) : ICommand;

public class UpdateTagHandler : ICommandHandler<UpdateTagCommand>
{
    private readonly ITagsRepository _tagsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateTagHandler> _logger;

    public UpdateTagHandler(
        ITagsRepository tagsRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateTagHandler> logger)
    {
        _tagsRepository = tagsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTagCommand command, CancellationToken cancellationToken)
    {
        var tag = await _tagsRepository.GetByIdAsync(command.Id, cancellationToken);
        if (tag is null)
            return Result.Failure(ErrorCodes.TagNotFound);

        var tagUpdatedResult = tag.Update(command.Name, command.Description);
        if (tagUpdatedResult.IsFailure)
            return Result.Failure(tagUpdatedResult.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogResult("Update Tag", command.Id, Result.Success());

        return Result.Success();
    }
}
