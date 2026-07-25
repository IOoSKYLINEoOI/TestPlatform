using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Users.DTOs;

namespace TestPlatform.Application.Users.Features;

public sealed record CreateUserAccountCommand(
    CreateUserAccountRequest Request) : ICommand;

public sealed class CreateUserAccountHandler(
    IIdentityAccountProvisioner provisioner)
    : ICommandHandler<CreateUserAccountCommand, CreateUserAccountResponse>
{
    public async Task<Result<CreateUserAccountResponse>> Handle(
        CreateUserAccountCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var result = await provisioner.CreateAsync(
            new IdentityAccountProvisioningRequest(
                request.Username.Trim(),
                request.EmployeeNumber.Trim(),
                request.TemporaryPassword,
                request.Role),
            cancellationToken);

        return result.IsSuccess
            ? Result.Success(new CreateUserAccountResponse(
                result.Value.IdentityProviderUserId,
                result.Value.Username,
                result.Value.EmployeeNumber,
                result.Value.Role))
            : Result.Failure<CreateUserAccountResponse>(result.Error);
    }
}
