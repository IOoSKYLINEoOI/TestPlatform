using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Questions;
using TestPlatform.Contracts.Share;
using TestPlatform.Core.Tests;

namespace TestPlatform.Application.Tests.Features.AddTestQuestionCommand;

public record AddTestQuestionCommand(Guid Id, AddQuestionRequest Request) : ICommand;

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
            return accessResult;

        var test = accessResult.Value;

        bool questionExists = await _questionsRepository.ExistsAsync(command.Request.QuestionId, cancellationToken);
        if (questionExists is false)
            return Result.Failure(ErrorCodes.QuestionNotFound);

        var result = test.AddQuestion(command.Request.QuestionId, command.Request.Score);
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}