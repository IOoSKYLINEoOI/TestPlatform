using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Tests.DTOs;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.UpdateTestDetailsCommand;

public record UpdateTestCommand(Guid Id, UpdateTestDetailsRequest Request) : ICommand;

public class UpdateTestDetailsHandler : ICommandHandler<UpdateTestCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Test> _testAccessService;
    private readonly ILogger<UpdateTestDetailsHandler> _logger;

    public UpdateTestDetailsHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Test> testAccessService,
        ILogger<UpdateTestDetailsHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _testAccessService = testAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTestCommand command, CancellationToken cancellationToken)
    {
        var accessResult = await _testAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return accessResult;

        var test = accessResult.Value;

        if (command.Request.Title != null)
        {
            var changeTitleResult = test.ChangeTitle(command.Request.Title);
            if (changeTitleResult.IsFailure)
                return Result.Failure(changeTitleResult.Error);
        }

        if (command.Request.Description != null)
        {
            var changeDescriptionResult = test.ChangeDescription(command.Request.Description);
            if (changeDescriptionResult.IsFailure)
                return Result.Failure(changeDescriptionResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Test {TestId} title and description updated.", command.Id);

        return Result.Success();
    }
}