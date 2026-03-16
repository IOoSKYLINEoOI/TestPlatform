using TestPlatform.Contracts.Tests.DTOs;

namespace TestPlatform.Application.Tests;

public interface ITestsReadRepository
{
    Task<TestFullResponse?> ReadTestByIdAsync(Guid? id, bool includeCorrectAnswer, CancellationToken cancellationToken);

    Task<List<TestResponse>> ReadAllTestAsync(CancellationToken cancellationToken);
}