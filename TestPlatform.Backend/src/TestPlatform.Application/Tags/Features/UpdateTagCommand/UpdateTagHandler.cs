using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;
using TestPlatform.Core.Tags;

namespace TestPlatform.Application.Tags.Features.UpdateTagCommand;

public record UpdateTagCommand(Guid Id, string Name, string Description) : ICommand;

public class UpdateTagHandler : ICommandHandler<UpdateTagCommand>
{
    private readonly ITagsRepository _tagsRepository;
    private readonly ILogger<UpdateTagHandler> _logger;

    public UpdateTagHandler(ITagsRepository tagsRepository, ILogger<UpdateTagHandler> logger)
    {
        _tagsRepository = tagsRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTagCommand command, CancellationToken cancellationToken)
    {
        var tagUpdatedResult = Tag.CreateWithId(command.Id, command.Name, command.Description);
        if (tagUpdatedResult.IsFailure)
            return Result.Failure(tagUpdatedResult.Error);

        var tagUpdated = tagUpdatedResult.Value;

        var updatedResult = await _tagsRepository.UpdateAsync(
            tagUpdated.Id,
            tagUpdated.Name,
            tagUpdated.Description,
            cancellationToken);

        _logger.LogResult("Update Tag", command.Id, updatedResult);

        return updatedResult;
    }
}
