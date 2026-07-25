using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.DeleteTestQuestionCommand;

public record DeleteTestQuestionCommand(Guid Id, Guid QuestionId) : ICommand;

public class DeleteTestQuestionHandler : ICommandHandler<DeleteTestQuestionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Test> _testAccessService;
    private readonly ILogger<DeleteTestQuestionHandler> _logger;

    public DeleteTestQuestionHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Test> testAccessService,
        ILogger<DeleteTestQuestionHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _testAccessService = testAccessService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTestQuestionCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _testAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
        {
            return accessResult;
        }

        var test = accessResult.Value;

        var result = test.RemoveQuestion(command.QuestionId);
        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Test {TestId} question {QuestionId} deleted.", command.Id, command.QuestionId);

        return Result.Success();
    }
}
