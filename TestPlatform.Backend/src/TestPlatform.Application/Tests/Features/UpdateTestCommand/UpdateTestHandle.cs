using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;
using TestPlatform.Contracts.Tests.DTOs;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.UpdateTestCommand;

public record UpdateTestCommand(Guid Id, TestRequest Request) : ICommand;

public class UpdateTestHandle : ICommandHandler<UpdateTestCommand>
{
    private readonly ITestsRepository _testsRepository;
    private readonly ILogger<UpdateTestHandle> _logger;

    public UpdateTestHandle(ITestsRepository testsRepository, ILogger<UpdateTestHandle> logger)
    {
        _testsRepository = testsRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTestCommand command, CancellationToken cancellationToken)
    {
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

        _logger.LogResult("Update Test", command.Id, updateResult);

        return updateResult;
    }
}