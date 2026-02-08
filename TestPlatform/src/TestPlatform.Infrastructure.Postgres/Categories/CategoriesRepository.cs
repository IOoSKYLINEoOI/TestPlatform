using CSharpFunctionalExtensions;
using TestPlatform.Application.Categories;
using TestPlatform.Core.Categories;

namespace TestPlatform.Infrastructure.Postgres.Categories;

public class CategoriesRepository : ICategoriesRepository
{
    public Task<Result<Guid>> AddAsync(Category category, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<Result> UpdateAsync(Guid id, string name, string description, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task<Result> DeleteAsync(Guid categoryId, CancellationToken cancellationToken) => throw new NotImplementedException();
}