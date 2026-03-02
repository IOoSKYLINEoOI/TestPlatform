using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Extensions;

namespace TestPlatform.Application.Questions.Features.DeleteQuestionCommand;

public record DeleteQuestionCommand(Guid Id) : ICommand;

public class DeleteQuestionHandler : ICommandHandler<DeleteQuestionCommand>
{
    private readonly IQuestionsRepository _questionsRepository;
    private readonly ILogger<DeleteQuestionHandler> _logger;

    public DeleteQuestionHandler(IQuestionsRepository questionsRepository, ILogger<DeleteQuestionHandler> logger)
    {
        _questionsRepository = questionsRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteQuestionCommand command, CancellationToken cancellationToken)
    {
        var result = await _questionsRepository.DeleteAsync(command.Id, cancellationToken);

        _logger.LogResult("Delete Question", command.Id, result);

        return result;
    }
}