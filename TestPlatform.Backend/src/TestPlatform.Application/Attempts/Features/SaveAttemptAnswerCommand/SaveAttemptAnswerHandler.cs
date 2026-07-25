using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Attempts.Extensions;
using TestPlatform.Application.Attempts.Services;
using TestPlatform.Contracts.Attempts.DTOs.AttemptAnswer.Request;
using TestPlatform.Core.Attempts;
using TestPlatform.Core.Attempts.Enums;


namespace TestPlatform.Application.Attempts.Features.SaveAttemptAnswerCommand;

public record SaveAttemptAnswerCommand(Guid AttemptId, AttemptAnswerRequest Request) : ICommand;

public class SaveAttemptAnswerHandler : ICommandHandler<SaveAttemptAnswerCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Attempt> _attemptAccessService;
    private readonly AttemptQuestionLoader _questionLoader;

    public SaveAttemptAnswerHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Attempt> attemptAccessService,
        AttemptQuestionLoader questionLoader)
    {
        _unitOfWork = unitOfWork;
        _attemptAccessService = attemptAccessService;
        _questionLoader = questionLoader;
    }

    public async Task<Result> Handle(SaveAttemptAnswerCommand command, CancellationToken cancellationToken)
    {
        var accessResult = await _attemptAccessService.GetForModifyAsync(command.AttemptId, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure(accessResult.Error);
        }

        var attempt = accessResult.Value;

        var answerResult = AttemptAnswerMappingExtensions.ToDomain(command.Request);
        if (answerResult.IsFailure)
        {
            return Result.Failure(answerResult.Error);
        }

        var selection = attempt.QuestionSelections
            .Where(x => x.QuestionId == answerResult.Value.QuestionId)
            .ToList();
        if (selection.Count == 0)
        {
            return Result.Failure("attempt.question_not_in_attempt");
        }

        var questionResult = await _questionLoader.LoadAsync(selection, cancellationToken);
        if (questionResult.IsFailure)
        {
            return Result.Failure(questionResult.Error);
        }

        var validation = AttemptAnswerValidator.Validate(
            questionResult.Value.Single().Question,
            answerResult.Value);
        if (validation.IsFailure)
        {
            return validation;
        }

        var saveResult = attempt.SaveAnswer(answerResult.Value);
        if (saveResult.IsFailure)
        {
            if (attempt.Status == AttemptStatus.EXPIRED)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return saveResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
