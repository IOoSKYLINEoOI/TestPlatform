using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Categories.DTOs;

namespace TestPlatform.Application.Categories.Features.GetByIdCategoryQuery;

public record GetByIdCategoryQuery(Guid Id) : IQuery;

public class GetByICategoryHandler : IQueryHandler<CategoryResponse, GetByIdCategoryQuery>
{
    private readonly IReadCategoriesDbContext _categoriesDbContext;
    private readonly ILogger<GetByICategoryHandler> _logger;

    public GetByICategoryHandler(IReadCategoriesDbContext categoriesDbContext, ILogger<GetByICategoryHandler> logger)
    {
        _categoriesDbContext = categoriesDbContext;
        _logger = logger;
    }

    public async Task<CategoryResponse?> Handle(GetByIdCategoryQuery query, CancellationToken cancellationToken)
    {
        var category = await _categoriesDbContext.ReadCategories
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);

        if (category == null)
            _logger.LogWarning("Category with id {Id} not found", query.Id);
        else
            _logger.LogInformation("Get Category with id {Id}", query.Id);

        return category;
    }
}
