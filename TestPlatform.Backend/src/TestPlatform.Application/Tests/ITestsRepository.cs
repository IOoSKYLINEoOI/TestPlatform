using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests;

public interface ITestsRepository
{
    Task<Test?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Test test, CancellationToken cancellationToken);
}
