using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Categories;
using TestPlatform.Contracts.Categories.DTOs;
using TestPlatform.Infrastructure.Postgres.Entities;

namespace TestPlatform.Infrastructure.Postgres.Categories;

public class CategoriesDbContext(DbContextOptions<CategoriesDbContext> options) : DbContext(options), IReadCategoriesDbContext
{
    public DbSet<CategoryEntity> Categories { get; set; }

    public IQueryable<CategoryResponse> ReadCategories => Categories
        .Select(x => new CategoryResponse(x.Id, x.Name, x.Description))
        .AsNoTracking()
        .AsQueryable();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CategoriesConfiguration());
    }
}