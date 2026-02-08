using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;
using TestPlatform.Core.Categories;

namespace TestPlatform.Application.Categories.Features.UpdateCategoryCommand;

public record UpdateCategoryCommand(Guid Id, string Name, string Description) : ICommand;

public class UpdateCategoryHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly ICategoriesRepository _categoriesRepository;
    private readonly ILogger<UpdateCategoryHandler> _logger;

    public UpdateCategoryHandler(ICategoriesRepository categoriesRepository, ILogger<UpdateCategoryHandler> logger)
    {
        _categoriesRepository = categoriesRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var categoryUpdatedResult = Category.CreateWithId(command.Id, command.Name, command.Description);
        if (categoryUpdatedResult.IsFailure)
            return Result.Failure(categoryUpdatedResult.Error);

        var categoryUpdated = categoryUpdatedResult.Value;

        var updatedResult = await _categoriesRepository.UpdateAsync(
            categoryUpdated.Id,
            categoryUpdated.Name,
            categoryUpdated.Description,
            cancellationToken);

        _logger.LogResult("Update Category", command.Id, updatedResult);

        return updatedResult;
    }
}
