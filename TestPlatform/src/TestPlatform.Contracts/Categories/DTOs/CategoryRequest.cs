using System.ComponentModel.DataAnnotations;

namespace TestPlatform.Contracts.Categories.DTOs;

public record CategoryRequest(
    string Name,
    string Description);