using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests;

public interface ITestsReadDbContext
{
    IQueryable<Test> ReadTests { get; }
}