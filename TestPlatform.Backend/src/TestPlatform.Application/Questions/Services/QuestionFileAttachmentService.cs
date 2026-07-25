using CSharpFunctionalExtensions;
using TestPlatform.Application.Files;
using TestPlatform.Core.Questions;

namespace TestPlatform.Application.Questions.Services;

public sealed class QuestionFileAttachmentService(IFileAssetsRepository fileAssetsRepository)
{
    public async Task<Result> AttachNewFilesAsync(
        Question question,
        IReadOnlyCollection<Guid> previousFileIds,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var newFileIds = question.GetReferencedFileIds()
            .Except(previousFileIds)
            .ToList();

        if (newFileIds.Count == 0)
        {
            return Result.Success();
        }

        var files = await fileAssetsRepository.GetByIdsAsync(newFileIds, cancellationToken);
        if (files.Count != newFileIds.Count)
        {
            return Result.Failure("file.not_found");
        }

        foreach (var file in files)
        {
            var attachResult = file.Attach(userId);
            if (attachResult.IsFailure)
            {
                return attachResult;
            }
        }

        return Result.Success();
    }
}
