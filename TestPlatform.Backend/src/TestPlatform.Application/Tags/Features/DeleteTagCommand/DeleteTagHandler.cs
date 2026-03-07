using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;

namespace TestPlatform.Application.Tags.Features.DeleteTagCommand;

public record DeleteTagCommand(Guid Id) : ICommand;

public class DeleteTagHandler : ICommandHandler<DeleteTagCommand>
{
    private readonly ITagsRepository _tagsRepository;
    private readonly ILogger<DeleteTagHandler> _logger;

    public DeleteTagHandler(ITagsRepository tagsRepository, ILogger<DeleteTagHandler> logger)
    {
        _tagsRepository = tagsRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTagCommand command, CancellationToken cancellationToken)
    {
        var result = await _tagsRepository.DeleteAsync(command.Id, cancellationToken);

        _logger.LogResult("Delete Tag", command.Id, result);

        return result;
    }
}