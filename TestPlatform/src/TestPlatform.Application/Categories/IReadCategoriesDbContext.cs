using TestPlatform.Contracts.Categories.DTOs;

namespace TestPlatform.Application.Categories;

public interface IReadCategoriesDbContext
{
    IQueryable<CategoryResponse> ReadCategories { get; }
}