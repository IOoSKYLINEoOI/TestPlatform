using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Contracts.Categories.DTOs;
using TestPlatform.Contracts.CategoryDTOs;
using TestPlatform.Core.Categories;

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

        var categoryIdResult = await _categoriesRepository.AddAsync(categoryResult.Value, cancellationToken);

        _logger.LogInformation("Added Category with id {categoryId}", categoryIdResult.Value);

        return Result.Success(categoryIdResult.Value);
    }

    public async Task<Result<Category>> GetCategoryById(Guid categoryId,  CancellationToken cancellationToken)
    {
        var categoryResult = await _categoriesRepository.GetByIdAsync(categoryId, cancellationToken);

        if (categoryResult.IsFailure)
            return Result.Failure<Category>(categoryResult.Error);

        _logger.LogInformation("Getting category with id {categoryId}", categoryId);

        return Result.Success(categoryResult.Value);
    }

    public async Task<IReadOnlyCollection<Category>> GetAllCategories(CancellationToken cancellationToken)
    {
        var categories = await _categoriesRepository.GetAllAsync(cancellationToken);

        _logger.LogInformation("Getting all categories");

        return categories;
    }

    public async Task<Result> Update(Guid id, string name, string description, CancellationToken cancellationToken)
    {
        var categoryResult = await _categoriesRepository.GetByIdAsync(id, cancellationToken);

        if (categoryResult.IsFailure)
            return Result.Failure(categoryResult.Error);

        var updateResult = categoryResult.Value.Update(name, description);
        if(updateResult.IsFailure)
            return Result.Failure(updateResult.Error);

        var updatedResult = await _categoriesRepository.UpdateAsync(categoryResult.Value, cancellationToken);

        _logger.LogInformation("Updated Category with id {categoryId}", categoryResult.Value.Id);

        return updatedResult;
    }

    public async Task<Result> Delete(Guid categoryId, CancellationToken cancellationToken)
    {
        var categoryResult = await _categoriesRepository.GetByIdAsync(categoryId, cancellationToken);

        if (categoryResult.IsFailure)
            return Result.Failure(categoryResult.Error);

        _logger.LogInformation("Delete Category with id {categoryId}", categoryResult.Value.Id);

        return await _categoriesRepository.DeleteAsync(categoryId, cancellationToken);
    }
}