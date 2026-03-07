using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;

namespace TestPlatform.Application.Tests.Features.DeleteTestCommand;

public record DeleteTestCommand(Guid Id) : ICommand;

public class DeleteTestHandler : ICommandHandler<DeleteTestCommand>
{
    private readonly ITestsRepository _testsRepository;
    private readonly ILogger<DeleteTestHandler> _logger;

    public DeleteTestHandler(ITestsRepository testsRepository, ILogger<DeleteTestHandler> logger)
    {
        _testsRepository = testsRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTestCommand command, CancellationToken cancellationToken)
    {
        var result = await _testsRepository.DeleteAsync(command.Id, cancellationToken);

        _logger.LogResult("Delete Test", command.Id, result);

        return result;
    }
}