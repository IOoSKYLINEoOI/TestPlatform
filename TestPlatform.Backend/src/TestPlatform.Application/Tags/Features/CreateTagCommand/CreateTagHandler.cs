using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;
using TestPlatform.Core.Tags;

namespace TestPlatform.Application.Tags.Features.CreateTagCommand;

public record CreateTagCommand(string Name, string Description) : ICommand;

public class CreateTagHandler : ICommandHandler<Guid, CreateTagCommand>
{
    private readonly ITagsRepository _tagsRepository;
    private readonly ILogger<CreateTagHandler> _logger;

    public CreateTagHandler(ITagsRepository tagsRepository, ILogger<CreateTagHandler> logger)
    {
        _tagsRepository = tagsRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateTagCommand command, CancellationToken cancellationToken)
    {
        var tagResult = Tag.Create(command.Name, command.Description);

        if(tagResult.IsFailure)
            return Result.Failure<Guid>(tagResult.Error);

        var tagIdResult = await _tagsRepository.AddAsync(tagResult.Value, cancellationToken);
        if (tagIdResult.IsFailure)
        {
            _logger.LogWarning("Failed to create tag: {Error}", tagIdResult.Error);

            return Result.Failure<Guid>(tagIdResult.Error);
        }

        _logger.LogResult("Create Tag", tagIdResult.Value, tagIdResult);

        return Result.Success(tagIdResult.Value);
    }
}