using CSharpFunctionalExtensions;
using TestPlatform.Core.Categories;

namespace TestPlatform.Application.Categories;

public interface ICategoriesRepository
{
    Task<Result<Guid>> AddAsync(Category category, CancellationToken cancellationToken);

    Task<Result<Category>> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Category>> GetAllAsync(CancellationToken cancellationToken);

    Task<Result> UpdateAsync(Category category, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid categoryId, CancellationToken cancellationToken);
}