namespace TestPlatform.Contracts.Categories.DTOs;

public record CategoryResponse(
    Guid Id,
    string Name,
    string Description);