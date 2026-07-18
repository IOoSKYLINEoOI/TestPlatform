using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;
using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Tags.Features.CreateTagCommand;

public record CreateTagCommand(string Name, string Description) : ICommand;

public class CreateTagHandler : ICommandHandler<CreateTagCommand, Guid>
{
    private readonly ITagsRepository _tagsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTagHandler> _logger;

    public CreateTagHandler(
        ITagsRepository tagsRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateTagHandler> logger)
    {
        _tagsRepository = tagsRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateTagCommand command, CancellationToken cancellationToken)
    {
        var tagResult = Tag.Create(command.Name, command.Description);

        if (tagResult.IsFailure)
            return Result.Failure<Guid>(tagResult.Error);

        await _tagsRepository.AddAsync(tagResult.Value, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogResult("Create Tag", tagResult.Value.Id, tagResult);

        return Result.Success(tagResult.Value.Id);
    }
}