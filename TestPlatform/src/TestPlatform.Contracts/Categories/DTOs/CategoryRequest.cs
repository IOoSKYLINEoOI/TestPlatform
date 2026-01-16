using System.ComponentModel.DataAnnotations;

namespace TestPlatform.Contracts.CategoryDTOs;

public record CategoryRequest(
    [Required] string Name,
    [Required] string Description);