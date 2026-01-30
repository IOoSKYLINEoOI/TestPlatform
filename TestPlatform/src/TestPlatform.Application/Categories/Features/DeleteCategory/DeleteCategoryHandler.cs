using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;

namespace TestPlatform.Application.Categories.Features.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : ICommand;

public class DeleteCategoryHandler : ICommandHandler<DeleteCategoryCommand>
{
    private readonly ICategoriesRepository _categoriesRepository;
    private readonly ILogger<DeleteCategoryHandler> _logger;

    public DeleteCategoryHandler(ICategoriesRepository categoriesRepository, ILogger<DeleteCategoryHandler> logger)
    {
        _categoriesRepository = categoriesRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await _categoriesRepository.DeleteAsync(command.Id, cancellationToken);

        _logger.LogResult("Delete Category", command.Id, result);

        return result;
    }
}