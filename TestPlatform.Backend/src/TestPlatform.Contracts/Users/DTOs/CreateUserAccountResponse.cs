namespace TestPlatform.Contracts.Users.DTOs;

public sealed record CreateUserAccountResponse(
    string IdentityProviderUserId,
    string Username,
    string EmployeeNumber,
    string Role);
