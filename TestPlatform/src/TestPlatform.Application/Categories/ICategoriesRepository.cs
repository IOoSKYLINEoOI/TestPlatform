using CSharpFunctionalExtensions;
using TestPlatform.Core.Categories;

namespace TestPlatform.Application.Categories;

public interface ICategoriesRepository
{
    Task<Result<Guid>> AddAsync(Category category, CancellationToken cancellationToken);

    Task<Result> UpdateAsync(Guid id, string name, string description, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid categoryId, CancellationToken cancellationToken);
}