using TestPlatform.Contracts.Categories.DTOs;

namespace TestPlatform.Application.Categories;

public interface IReadCategoriesRepository
{
    Task<CategoryResponse?> ReadCategoryByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<CategoryResponse>> ReadAllCategoriesAsync(CancellationToken cancellationToken);
}