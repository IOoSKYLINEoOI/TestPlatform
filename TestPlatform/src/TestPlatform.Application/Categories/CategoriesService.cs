using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Contracts.CategoryDTOs;
using TestPlatform.Core.Models.Category;

namespace TestPlatform.Application.Categories;

public class CategoriesService
{
    private readonly ICategoriesRepository _categoriesRepository;
    private readonly ILogger<CategoriesService> _logger;

    public CategoriesService(ICategoriesRepository categoriesRepository, ILogger<CategoriesService> logger)
    {
        _categoriesRepository = categoriesRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Add(CategoryRequest categoryRequest, CancellationToken cancellationToken)
    {
        var categoryResult = Category.Create(categoryRequest.Name, categoryRequest.Description);

        if(categoryResult.IsFailure)
            return Result.Failure<Guid>(categoryResult.Error);

        var categoryId = await _categoriesRepository.AddAsync(categoryResult.Value, cancellationToken);

        _logger.LogInformation("Added Category with id {categoryId}", categoryId);

        return Result.Success(categoryId.Value);
    }

    public async Task<Result<Category>> GetCategoryById(int categoryId)
    {
        var categoryResult = await _categoriesRepository.GetByIdAsync(categoryId);

        if (categoryResult.IsFailure)
            return Result.Failure<Category>(categoryResult.Error);

        return Result.Success(categoryResult.Value);
    }

    public async Task<List<Category>> GetAllCategories()
    {
        var categories = await _categoriesRepository.GetAllAsync();

        return categories;
    }

    public async Task<Result> Update(int id, string name, string description)
    {
        var categoryResult = await _categoriesRepository.GetByIdAsync(id);

        if (categoryResult.IsFailure)
            return Result.Failure(categoryResult.Error);

        var updatedCategory = Category.Create(name, description);

        if (updatedCategory.IsFailure)
            return Result.Failure(updatedCategory.Error);

        var updatedResult = await _categoriesRepository.UpdateAsync(id, updatedCategory.Value);

        return updatedResult;
    }

    public async Task<Result> Delete(int categoryId)
    {
        var categoryResult = await _categoriesRepository.GetByIdAsync(categoryId);

        if (categoryResult.IsFailure)
            return Result.Failure(categoryResult.Error);

        return await _categoriesRepository.DeleteAsync(categoryId);
    }
}