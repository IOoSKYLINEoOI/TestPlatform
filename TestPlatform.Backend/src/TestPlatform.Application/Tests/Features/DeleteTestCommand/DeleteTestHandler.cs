using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Abstractions.Enums;
using TestPlatform.Application.Extensions;
using TestPlatform.Application.Questions;
using TestPlatform.Contracts.Users.DTOs;

namespace TestPlatform.Application.Tests.Features.DeleteTestCommand;

public record DeleteTestCommand(Guid Id, CurrentUserDto CurrentUser) : ICommand;

public class DeleteTestHandler : ICommandHandler<DeleteTestCommand>
{
    private readonly ITestsRepository _testsRepository;
    private readonly ITestsReadRepository _testsReadRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<DeleteTestHandler> _logger;

    public DeleteTestHandler(
        ITestsRepository testsRepository,
        ITestsReadRepository testsReadRepository,
        IImageStorageService imageStorageService,
        ILogger<DeleteTestHandler> logger)
    {
        _testsRepository = testsRepository;
        _testsReadRepository = testsReadRepository;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTestCommand command, CancellationToken cancellationToken)
    {
        var testExisting = await _testsReadRepository.ReadTestByIdAsync(command.Id, false, cancellationToken);
        if (testExisting == null)
            return Result.Failure($"Could not find test with id {command.Id}");

        if (!command.CurrentUser.IsAdmin && command.CurrentUser.Id != testExisting.AuthorId)
            return Result.Failure("Forbidden");

        var result = await _testsRepository.DeleteAsync(command.Id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(testExisting.CoverImageName))
        {
            var deleteResult = await _imageStorageService.DeletePermanentAsync(ImageFolder.TESTS, testExisting.CoverImageName);
            if (deleteResult.IsFailure)
                _logger.LogWarning("Failed to delete test cover image {ImageName}: {Error}", testExisting.CoverImageName, deleteResult.Error);
            else
                _logger.LogInformation("Deleted test cover image {ImageName}", testExisting.CoverImageName);
        }

        _logger.LogResult("Delete Test", command.Id, result);

        return result;
    }
}