using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Attempts;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Infrastructure.Postgres.Attempts;

public sealed class AttemptExpirationService(
    TestPlatformDbContext dbContext) : IAttemptExpirationService
{
    public Task<int> ExpireOverdueAsync(
        DateTime now,
        CancellationToken cancellationToken)
    {
        return dbContext.Attempts
            .Where(attempt => attempt.Status == AttemptStatus.STARTED
                              && attempt.Deadline < now)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(attempt => attempt.Status, AttemptStatus.EXPIRED)
                    .SetProperty(attempt => attempt.FinishedAt, now),
                cancellationToken);
    }
}
