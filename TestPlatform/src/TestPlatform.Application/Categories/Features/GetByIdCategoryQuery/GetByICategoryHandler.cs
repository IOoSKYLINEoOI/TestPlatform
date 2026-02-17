using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Categories.DTOs;

namespace TestPlatform.Application.Categories.Features.GetByIdCategoryQuery;

public record GetByIdCategoryQuery(Guid Id) : IQuery;

public class GetByICategoryHandler : IQueryHandler<CategoryResponse, GetByIdCategoryQuery>
{
    private readonly IReadCategoriesRepository _repository;
    private readonly ILogger<GetByICategoryHandler> _logger;

    public GetByICategoryHandler(IReadCategoriesRepository repository, ILogger<GetByICategoryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CategoryResponse?> Handle(GetByIdCategoryQuery query, CancellationToken cancellationToken)
    {
        var category = await _repository.ReadCategoryByIdAsync(query.Id, cancellationToken);

        if (category == null)
            _logger.LogWarning("Category with id {Id} not found", query.Id);
        else
            _logger.LogInformation("Get Category with id {Id}", query.Id);

        return category;
    }
}
