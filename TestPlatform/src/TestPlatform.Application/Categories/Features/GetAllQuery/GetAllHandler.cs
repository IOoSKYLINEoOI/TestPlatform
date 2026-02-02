using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Categories.DTOs;

namespace TestPlatform.Application.Categories.Features.GetAllQuery;

public record GetAllQuery() : IQuery;

public class GetAllHandler : IQueryHandler<List<CategoryResponse>, GetAllQuery>
{
    private readonly IReadCategoriesDbContext _categoriesDbContext;
    private readonly ILogger<GetAllHandler> _logger;

    public GetAllHandler(IReadCategoriesDbContext categoriesDbContext, ILogger<GetAllHandler> logger)
    {
        _categoriesDbContext = categoriesDbContext;
        _logger = logger;
    }

    public async Task<List<CategoryResponse>?> Handle(GetAllQuery query, CancellationToken cancellationToken)
    {
        var categories = await _categoriesDbContext.ReadCategories
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} categories", categories.Count);

        return categories;
    }
}
