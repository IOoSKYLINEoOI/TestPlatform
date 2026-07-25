using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Common.Error;
using TestPlatform.Application.Questions;
using TestPlatform.Contracts.Exams.DTOs;
using TestPlatform.Core.Exams;
using TestPlatform.Core.Exams.Enums;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Application.Exams.Features.ManageExamSections;

public record AddExamSectionCommand(Guid ExamId, CreateExamSectionRequest Request) : ICommand;
public record RemoveExamSectionCommand(Guid ExamId, Guid SectionId) : ICommand;
public record UpdateExamSectionCommand(Guid ExamId, Guid SectionId, UpdateExamSectionRequest Request) : ICommand;
public record AddQuestionToExamSectionCommand(Guid ExamId, Guid SectionId, Guid QuestionId) : ICommand;
public record RemoveQuestionFromExamSectionCommand(Guid ExamId, Guid SectionId, Guid QuestionId) : ICommand;
public record UpdateExamAttemptsLimitCommand(Guid ExamId, int AttemptsLimit) : ICommand;
public record UpdateExamReviewPolicyCommand(Guid ExamId, ExamReviewPolicyDto ReviewPolicy) : ICommand;

public sealed class AddExamSectionHandler(IAccessService<Exam> accessService, IUnitOfWork unitOfWork)
    : ICommandHandler<AddExamSectionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddExamSectionCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.GetForModifyAsync(command.ExamId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure<Guid>(access.Error);
        }

        var result = access.Value.AddSection(
            command.Request.Name,
            command.Request.QuestionsToSelect,
            command.Request.ScorePerQuestion);
        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(result.Value);
    }
}

public sealed class UpdateExamSectionHandler(IAccessService<Exam> accessService, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateExamSectionCommand>
{
    public async Task<Result> Handle(UpdateExamSectionCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.GetForModifyAsync(command.ExamId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure(access.Error);
        }

        var result = access.Value.UpdateSection(
            command.SectionId,
            command.Request.Name,
            command.Request.QuestionsToSelect,
            command.Request.ScorePerQuestion);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class RemoveExamSectionHandler(IAccessService<Exam> accessService, IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveExamSectionCommand>
{
    public async Task<Result> Handle(RemoveExamSectionCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.GetForModifyAsync(command.ExamId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure(access.Error);
        }

        var result = access.Value.RemoveSection(command.SectionId);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class AddQuestionToExamSectionHandler(
    IAccessService<Exam> accessService,
    IQuestionsRepository questionsRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddQuestionToExamSectionCommand>
{
    public async Task<Result> Handle(AddQuestionToExamSectionCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.GetForModifyAsync(command.ExamId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure(access.Error);
        }

        var question = await questionsRepository.GetByIdAsync(command.QuestionId, cancellationToken);
        if (question is null)
        {
            return Result.Failure(ErrorCodes.QuestionNotFound);
        }

        if (question.Status != QuestionStatus.Published)
        {
            return Result.Failure("question.not_published");
        }

        var result = access.Value.AddQuestionToSection(command.SectionId, command.QuestionId);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class RemoveQuestionFromExamSectionHandler(IAccessService<Exam> accessService, IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveQuestionFromExamSectionCommand>
{
    public async Task<Result> Handle(RemoveQuestionFromExamSectionCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.GetForModifyAsync(command.ExamId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure(access.Error);
        }

        var result = access.Value.RemoveQuestionFromSection(command.SectionId, command.QuestionId);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class UpdateExamAttemptsLimitHandler(IAccessService<Exam> accessService, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateExamAttemptsLimitCommand>
{
    public async Task<Result> Handle(UpdateExamAttemptsLimitCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.GetForModifyAsync(command.ExamId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure(access.Error);
        }

        var result = access.Value.ChangeAttemptsLimit(command.AttemptsLimit);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class UpdateExamReviewPolicyHandler(IAccessService<Exam> accessService, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateExamReviewPolicyCommand>
{
    public async Task<Result> Handle(UpdateExamReviewPolicyCommand command, CancellationToken cancellationToken)
    {
        var access = await accessService.GetForModifyAsync(command.ExamId, cancellationToken);
        if (access.IsFailure)
        {
            return Result.Failure(access.Error);
        }

        var result = access.Value.ChangeReviewPolicy((ExamReviewPolicy)command.ReviewPolicy);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
