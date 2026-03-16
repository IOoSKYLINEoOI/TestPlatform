using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Abstractions.Enums;
using TestPlatform.Application.Extensions;
using TestPlatform.Contracts.Tests.DTOs;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.UpdateTestCommand;

public record UpdateTestCommand(Guid Id, TestRequest Request) : ICommand;

public class UpdateTestHandle : ICommandHandler<UpdateTestCommand>
{
    private readonly ITestsRepository _testsRepository;
    private readonly ITestsReadRepository _testsReadRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<UpdateTestHandle> _logger;

    public UpdateTestHandle(
        ITestsRepository testsRepository,
        ITestsReadRepository testsReadRepository,
        IImageStorageService imageStorageService,
        ILogger<UpdateTestHandle> logger)
    {
        _testsRepository = testsRepository;
        _testsReadRepository = testsReadRepository;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTestCommand command, CancellationToken cancellationToken)
    {
        var testExisting = await _testsReadRepository.ReadTestByIdAsync(command.Id, false, cancellationToken);
        if(testExisting is null)
            return Result.Failure($"Test with id {command.Id} NOT Found");

        var testUpdatedResult = Test.CreateWithId(
            command.Id,
            command.Request.Name,
            command.Request.TimeLimitSeconds,
            command.Request.Description,
            command.Request.AuthorId,
            command.Request.CoverImageUrl);
        if(testUpdatedResult.IsFailure)
            return Result.Failure(testUpdatedResult.Error);

        var testUpdated = testUpdatedResult.Value;

        foreach (var question in command.Request.QuestionsIds.ToHashSet())
            testUpdated.AddQuestion(question);

        var updateResult = await _testsRepository.UpdateAsync(testUpdated, cancellationToken);

        if (testExisting.CoverImageName != testUpdated.CoverImageName)
        {
            if (testUpdated.CoverImageName is not null)
            {
                await _imageStorageService.MoveToPermanentAsync(testUpdated.CoverImageName, ImageFolder.TESTS, cancellationToken);

                _logger.LogInformation("Moved CoverImage {testUpdated.CoverImageName}", testUpdated.CoverImageName);
            }

            if (testExisting.CoverImageName is not null)
            {
                await _imageStorageService.DeletePermanentAsync(ImageFolder.TESTS, testExisting.CoverImageName);

                _logger.LogInformation("Deleted CoverImage {testExisting.CoverImageName}", testExisting.CoverImageName);
            }
        }

        _logger.LogResult("Update Test", command.Id, updateResult);

        return updateResult;
    }
}