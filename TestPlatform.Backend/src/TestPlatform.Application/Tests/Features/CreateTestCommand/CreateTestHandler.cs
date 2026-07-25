using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Extensions;
using TestPlatform.Application.Users;
using TestPlatform.Contracts.Tests.DTOs;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.CreateTestCommand;

public record CreateTestCommand(TestRequest Request) : ICommand;

public class CreateTestHandler : ICommandHandler<CreateTestCommand, Guid>
{
    private readonly ITestsRepository _testsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ILogger<CreateTestHandler> _logger;

    public CreateTestHandler(
        ITestsRepository testsRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUser,
        ILogger<CreateTestHandler> logger)
    {
        _testsRepository = testsRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateTestCommand command, CancellationToken cancellationToken)
    {
        var user = _currentUser.User;

        if (user is null)
        {
            _logger.LogWarning("Unauthorized access attempt to create exam.");
            return Result.Failure<Guid>(ErrorCodes.Unauthorized);
        }

        var testResult = Test.Create(
            command.Request.Title,
            command.Request.Description,
            user.Id);

        if (testResult.IsFailure)
        {
            return Result.Failure<Guid>(testResult.Error);
        }

        await _testsRepository.AddAsync(testResult.Value, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogResult("Create Test", testResult.Value.Id, testResult);

        return Result.Success(testResult.Value.Id);
    }
}
