namespace TestPlatform.Application.Users;

public sealed record CurrentIdentity(
    Guid Id,
    string EmployeeNumber,
    bool IsAdmin);
