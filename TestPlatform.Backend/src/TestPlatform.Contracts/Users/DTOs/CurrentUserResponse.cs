namespace TestPlatform.Contracts.Users.DTOs;

public record CurrentUserResponse(
    Guid Id,
    string EmployeeNumber,
    bool IsAdmin);
