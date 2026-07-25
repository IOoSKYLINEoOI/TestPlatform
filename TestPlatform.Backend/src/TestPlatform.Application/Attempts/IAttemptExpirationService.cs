namespace TestPlatform.Application.Attempts;

public interface IAttemptExpirationService
{
    Task<int> ExpireOverdueAsync(
        DateTime now,
        CancellationToken cancellationToken);
}
