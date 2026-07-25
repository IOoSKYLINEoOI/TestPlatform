using CSharpFunctionalExtensions;

namespace TestPlatform.Application.Users;

public interface IIdentityAccountProvisioner
{
    Task<Result<ProvisionedIdentityAccount>> CreateAsync(
        IdentityAccountProvisioningRequest request,
        CancellationToken cancellationToken);
}

public sealed record IdentityAccountProvisioningRequest(
    string Username,
    string EmployeeNumber,
    string TemporaryPassword,
    string Role);

public sealed record ProvisionedIdentityAccount(
    string IdentityProviderUserId,
    string Username,
    string EmployeeNumber,
    string Role);
