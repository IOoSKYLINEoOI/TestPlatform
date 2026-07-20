namespace TestPlatform.Contracts.Users.DTOs;

public record CurrentUserDto(
    Guid Id,
    string KeycloakId,
    string TabNumber,
    bool IsAdmin);