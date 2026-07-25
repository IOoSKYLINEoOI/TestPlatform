namespace TestPlatform.Contracts.Users.DTOs;

public sealed record CreateUserAccountRequest(
    string Username,
    string EmployeeNumber,
    string TemporaryPassword,
    string Role);
