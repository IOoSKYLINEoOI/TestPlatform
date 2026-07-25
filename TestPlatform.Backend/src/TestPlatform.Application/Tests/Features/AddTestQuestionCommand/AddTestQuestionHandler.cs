using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Questions;
using TestPlatform.Core.Questions.AnswerDefinition;
using TestPlatform.Core.Questions.Enums;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.AddTestQuestionCommand;

public record AddTestQuestionCommand(Guid Id, Guid QuestionId) : ICommand;

public class AddTestQuestionHandler : ICommandHandler<AddTestQuestionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Test> _testAccessService;
    private readonly IQuestionsRepository _questionsRepository;

    public AddTestQuestionHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Test> testAccessService,
        IQuestionsRepository questionsRepository)
    {
        _unitOfWork = unitOfWork;
        _testAccessService = testAccessService;
        _questionsRepository = questionsRepository;
    }

    public async Task<Result> Handle(AddTestQuestionCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _testAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
        {
            return accessResult;
        }

        var test = accessResult.Value;

        var question = await _questionsRepository.GetByIdAsync(command.QuestionId, cancellationToken);
        if (question is null)
        {
            return Result.Failure(ErrorCodes.QuestionNotFound);
        }

        if (question.Status != QuestionStatus.Published)
        {
            return Result.Failure("question.not_published");
        }

        var supportsCorrectnessOnly = question.AnswerDefinition switch
        {
            ChoiceAnswerDefinition choice => choice.EvaluationMode == EvaluationMode.Strict,
            MatchingAnswerDefinition matching => matching.Mode == EvaluationMode.Strict,
            _ => true,
        };
        if (!supportsCorrectnessOnly)
        {
            return Result.Failure("test.partial_evaluation_not_supported");
        }

        var result = test.AddQuestion(command.QuestionId);
        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
