namespace TestPlatform.Contracts.Tags.DTOs;

public record TagResponse(
    Guid Id,
    string Name,
    string? Description);