using CSharpFunctionalExtensions;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests;

public interface ITestsRepository
{
    Task<Result<Guid>> AddAsync(Test test, CancellationToken cancellationToken);

    Task<Result> UpdateAsync(Test test, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid testId, CancellationToken cancellationToken);
}