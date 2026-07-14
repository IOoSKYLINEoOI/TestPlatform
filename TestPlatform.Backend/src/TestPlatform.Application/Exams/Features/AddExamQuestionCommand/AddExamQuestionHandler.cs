using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Questions;
using TestPlatform.Contracts.Share;
using TestPlatform.Core.Exams;

namespace TestPlatform.Application.Exams.Features.AddExamQuestionCommand;

public record AddExamQuestionCommand(Guid Id, AddQuestionRequest Request) : ICommand;

public class AddExamQuestionHandler : ICommandHandler<AddExamQuestionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccessService<Exam> _examAccessService;
    private readonly IQuestionsRepository _questionsRepository;

    public AddExamQuestionHandler(
        IUnitOfWork unitOfWork,
        IAccessService<Exam> examAccessService,
        IQuestionsRepository questionsRepository)
    {
        _unitOfWork = unitOfWork;
        _examAccessService = examAccessService;
        _questionsRepository = questionsRepository;
    }

    public async Task<Result> Handle(AddExamQuestionCommand command, CancellationToken cancellationToken = default)
    {
        var accessResult = await _examAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
            return Result.Failure(accessResult.Error);

        var exam = accessResult.Value;

        bool questionExists = await _questionsRepository.ExistsAsync(command.Request.QuestionId, cancellationToken);
        if (questionExists is false)
            return Result.Failure(ErrorCodes.QuestionNotFound);

        var result = exam.AddQuestion(command.Request.QuestionId, command.Request.Score);
        if (result.IsFailure)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}