using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Abstractions.Enums;
using TestPlatform.Application.Extensions;
using TestPlatform.Contracts.Tests.DTOs;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.UpdateTestCommand;

public record UpdateTestCommand(Guid Id, TestRequest Request) : ICommand;

public class UpdateTestHandler : ICommandHandler<UpdateTestCommand>
{
    private readonly ITestsRepository _testsRepository;
    private readonly ITestsReadRepository _testsReadRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<UpdateTestHandler> _logger;

    public UpdateTestHandler(
        ITestsRepository testsRepository,
        ITestsReadRepository testsReadRepository,
        IImageStorageService imageStorageService,
        ILogger<UpdateTestHandler> logger)
    {
        _testsRepository = testsRepository;
        _testsReadRepository = testsReadRepository;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTestCommand command, CancellationToken cancellationToken)
    {
        var testExisting = await _testsReadRepository.ReadTestByIdAsync(command.Id, false, cancellationToken);
        if (testExisting is null)
            return Result.Failure($"Test with id {command.Id} not found");

        var testUpdatedResult = Test.CreateWithId(
            command.Id,
            command.Request.Name,
            command.Request.TimeLimitSeconds,
            command.Request.Description,
            command.Request.AuthorId,
            command.Request.CoverImageUrl);

        if (testUpdatedResult.IsFailure)
            return Result.Failure(testUpdatedResult.Error);

        var testUpdated = testUpdatedResult.Value;

        foreach (var questionId in command.Request.QuestionsIds.ToHashSet())
            testUpdated.AddQuestion(questionId);

        var updateResult = await _testsRepository.UpdateAsync(testUpdated, cancellationToken);
        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Error);

        if (testExisting.CoverImageName != testUpdated.CoverImageName)
        {
            if (!string.IsNullOrWhiteSpace(testUpdated.CoverImageName))
            {
                await _imageStorageService.MoveToPermanent(testUpdated.CoverImageName, ImageFolder.TESTS);
                _logger.LogInformation("Moved CoverImage {CoverImage}", testUpdated.CoverImageName);
            }

            if (!string.IsNullOrWhiteSpace(testExisting.CoverImageName))
            {
                await _imageStorageService.DeletePermanentAsync(ImageFolder.TESTS, testExisting.CoverImageName);
                _logger.LogInformation("Deleted CoverImage {CoverImage}", testExisting.CoverImageName);
            }
        }

        _logger.LogResult("Update Test", command.Id, updateResult);
        return updateResult;
    }
}