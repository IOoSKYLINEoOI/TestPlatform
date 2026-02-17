using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Categories;
using TestPlatform.Contracts.Categories.DTOs;

namespace TestPlatform.Infrastructure.Postgres.Categories;

public class ReadCategoriesRepository : IReadCategoriesRepository
{
    private readonly TestPlatformDbContext _context;

    public ReadCategoriesRepository(TestPlatformDbContext context)
    {
        _context = context;
    }


    public async Task<CategoryResponse?> ReadCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var categoryEntity = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (categoryEntity == null)
            return null;

        var category = new CategoryResponse(categoryEntity.Id, categoryEntity.Name, categoryEntity.Description);

        return category;
    }

    public async Task<List<CategoryResponse>> ReadAllCategoriesAsync(CancellationToken cancellationToken)
        => await _context.Categories
            .AsNoTracking()
            .Select(x => new CategoryResponse(x.Id, x.Name, x.Description))
            .ToListAsync(cancellationToken);
}