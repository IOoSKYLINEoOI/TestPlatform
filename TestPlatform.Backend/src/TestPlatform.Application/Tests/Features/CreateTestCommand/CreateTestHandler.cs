using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Abstractions.Enums;
using TestPlatform.Application.Extensions;
using TestPlatform.Application.Questions;
using TestPlatform.Contracts.Tests.DTOs;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.CreateTestCommand;

public record CreateTestCommand(TestRequest Request, Guid AuthorId) : ICommand;

public class CreateTestHandler : ICommandHandler<Guid, CreateTestCommand>
{
    private readonly ITestsRepository _testsRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<CreateTestHandler> _logger;

    public CreateTestHandler(
        ITestsRepository testsRepository,
        IImageStorageService imageStorageService,
        ILogger<CreateTestHandler> logger)
    {
        _testsRepository = testsRepository;
        _imageStorageService = imageStorageService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateTestCommand command, CancellationToken cancellationToken)
    {
        var testResult = Test.Create(
            command.Request.Name,
            command.Request.TimeLimitSeconds,
            command.Request.Description,
            command.AuthorId,
            command.Request.CoverImageUrl);

        if (testResult.IsFailure)
            return Result.Failure<Guid>(testResult.Error);

        var test = testResult.Value;

        foreach (var questionId in command.Request.QuestionsIds.ToHashSet())
            test.AddQuestion(questionId);

        var testIdResult = await _testsRepository.AddAsync(test, cancellationToken);
        if (testIdResult.IsFailure)
        {
            _logger.LogWarning("Failed to create Test: {Error}", testIdResult.Error);
            return Result.Failure<Guid>(testIdResult.Error);
        }

        if (!string.IsNullOrWhiteSpace(command.Request.CoverImageUrl))
        {
            var moveResult = await _imageStorageService.MoveToPermanent(command.Request.CoverImageUrl, ImageFolder.TESTS);
            if (moveResult.IsFailure)
            {
                _logger.LogWarning("Failed to move test cover image {Image}: {Error}", command.Request.CoverImageUrl, moveResult.Error);
                return Result.Failure<Guid>(moveResult.Error);
            }

            _logger.LogInformation("Moved test cover image {Image}", command.Request.CoverImageUrl);
        }

        _logger.LogResult("Create Test", testIdResult.Value, testIdResult);
        return Result.Success(testIdResult.Value);
    }
}