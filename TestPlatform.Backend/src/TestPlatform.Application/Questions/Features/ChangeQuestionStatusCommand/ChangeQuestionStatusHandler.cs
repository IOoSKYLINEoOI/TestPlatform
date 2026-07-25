using CSharpFunctionalExtensions;
using TestPlatform.Application.Abstractions;
using TestPlatform.Application.Files;
using TestPlatform.Core.Files;
using TestPlatform.Core.Questions;
using TestPlatform.Core.Questions.Enums;

namespace TestPlatform.Application.Questions.Features.ChangeQuestionStatusCommand;

public record ChangeQuestionStatusCommand(Guid Id, QuestionStatus Status) : ICommand;

public sealed class ChangeQuestionStatusHandler(
    IAccessService<Question> questionAccessService,
    IFileAssetsRepository fileAssetsRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<ChangeQuestionStatusCommand>
{
    public async Task<Result> Handle(ChangeQuestionStatusCommand command, CancellationToken cancellationToken)
    {
        var accessResult = await questionAccessService.GetForModifyAsync(command.Id, cancellationToken);
        if (accessResult.IsFailure)
        {
            return Result.Failure(accessResult.Error);
        }

        var question = accessResult.Value;

        if (command.Status == QuestionStatus.Published)
        {
            var referencedFileIds = question.GetReferencedFileIds();
            if (referencedFileIds.Count != 0)
            {
                var files = await fileAssetsRepository.GetByIdsAsync(referencedFileIds, cancellationToken);
                if (files.Count != referencedFileIds.Count || files.Any(file => file.Status != FileAssetStatus.Attached))
                {
                    return Result.Failure("question.file_unavailable");
                }
            }
        }

        var result = command.Status switch
        {
            QuestionStatus.Published => question.Publish(),
            QuestionStatus.Archived => question.Archive(),
            _ => Result.Failure("question.invalid_status"),
        };

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
