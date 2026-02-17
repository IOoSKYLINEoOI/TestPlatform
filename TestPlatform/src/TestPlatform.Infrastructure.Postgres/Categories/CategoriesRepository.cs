using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Categories;
using TestPlatform.Core.Categories;

namespace TestPlatform.Infrastructure.Postgres.Categories;

public class CategoriesRepository : ICategoriesRepository
{
    private readonly TestPlatformDbContext _context;

    public CategoriesRepository(TestPlatformDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> AddAsync(Category category, CancellationToken cancellationToken)
    {
        var categoryEntity = MapToEntity(category);

        await _context.Categories.AddAsync(categoryEntity,  cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(categoryEntity.Id);
    }

    public async Task<Result> UpdateAsync(Guid id, string name, string description, CancellationToken cancellationToken)
    {
        var categoryEntity = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);

        if (categoryEntity is null)
            return Result.Failure($"Category with id {id} not found");

        categoryEntity.Name = name;
        categoryEntity.Description = description;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var categoryEntity = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);

        if (categoryEntity is null)
            return Result.Failure($"Category with id {id} not found");

        _context.Categories.Remove(categoryEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static CategoryEntity MapToEntity(Category category) => new CategoryEntity()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
    };
}