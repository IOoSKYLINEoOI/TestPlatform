using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;
using TestPlatform.Core.Categories;

namespace TestPlatform.Application.Categories.Features.CreateCategoryCommand;

public record CreateCategoryCommand(string Name, string Description) : ICommand;

public class CreateCategoryHandler : ICommandHandler<Guid, CreateCategoryCommand>
{
    private readonly ICategoriesRepository _categoriesRepository;
    private readonly ILogger<CreateCategoryHandler> _logger;

    public CreateCategoryHandler(ICategoriesRepository categoriesRepository, ILogger<CreateCategoryHandler> logger)
    {
        _categoriesRepository = categoriesRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var categoryResult = Category.Create(command.Name, command.Description);

        if(categoryResult.IsFailure)
            return Result.Failure<Guid>(categoryResult.Error);

        var categoryIdResult = await _categoriesRepository.AddAsync(categoryResult.Value, cancellationToken);
        if (categoryIdResult.IsFailure)
        {
            _logger.LogWarning("Failed to create category: {Error}", categoryIdResult.Error);

            return Result.Failure<Guid>(categoryIdResult.Error);
        }

        _logger.LogResult("Create Category", categoryIdResult.Value, categoryIdResult);

        return Result.Success(categoryIdResult.Value);
    }
}