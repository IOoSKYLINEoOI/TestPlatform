using TestPlatform.Core.Attempts;

namespace TestPlatform.Application.Attempts;

public interface IAttemptsReadDbContext
{
    IQueryable<Attempt> ReadAttempts { get; }
}