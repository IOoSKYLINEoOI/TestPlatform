using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Users;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Services;

public class TestAccessService : IAccessService<Test>
{
    private readonly ITestsRepository _testsRepository;
    private readonly ICurrentUserAccessor _currentUser;
    private readonly ILogger<TestAccessService> _logger;

    public TestAccessService(
        ITestsRepository testsRepository,
        ICurrentUserAccessor currentUser,
        ILogger<TestAccessService> logger)
    {
        _testsRepository = testsRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<Test>> GetForModifyAsync(Guid id, CancellationToken ct)
    {
        var user = _currentUser.User;

        if (user is null)
        {
            _logger.LogWarning("Unauthorized access attempt to update test {TestId}", id);
            return Result.Failure<Test>(ErrorCodes.Unauthorized);
        }

        var test = await _testsRepository.GetByIdAsync(id, ct);

        if (test is null)
        {
            _logger.LogInformation("Test with {TestId} not found.", id);
            return Result.Failure<Test>(ErrorCodes.TestNotFound);
        }

        if (test.AuthorId != user.Id && !user.IsAdmin)
        {
            _logger.LogWarning("User {UserId} has no rights to update test {TestId}", user.Id, id);
            return Result.Failure<Test>(ErrorCodes.Forbidden);
        }

        return Result.Success(test);
    }
}
