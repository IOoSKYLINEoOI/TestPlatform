using System.Collections.Concurrent;
using CSharpFunctionalExtensions;
using TestPlatform.Application.Users;

namespace TestPlatform.Api.IntegrationTests.Infrastructure;

public sealed class TestIdentityAccountProvisioner : IIdentityAccountProvisioner
{
    private readonly ConcurrentDictionary<string, ProvisionedIdentityAccount> _accounts =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<Result<ProvisionedIdentityAccount>> CreateAsync(
        IdentityAccountProvisioningRequest request,
        CancellationToken cancellationToken)
    {
        if (_accounts.Values.Any(account =>
                string.Equals(
                    account.EmployeeNumber,
                    request.EmployeeNumber,
                    StringComparison.Ordinal)))
        {
            return Task.FromResult(Result.Failure<ProvisionedIdentityAccount>(
                IdentityAccountErrors.EmployeeNumberAlreadyExists));
        }

        var account = new ProvisionedIdentityAccount(
            Guid.NewGuid().ToString(),
            request.Username,
            request.EmployeeNumber,
            request.Role);

        return Task.FromResult(_accounts.TryAdd(request.Username, account)
            ? Result.Success(account)
            : Result.Failure<ProvisionedIdentityAccount>(
                IdentityAccountErrors.UsernameAlreadyExists));
    }
}
