using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Files;
using TestPlatform.Core.Files;

namespace TestPlatform.Infrastructure.Postgres.Files;

public class FileAssetsRepository(TestPlatformDbContext dbContext) : IFileAssetsRepository
{
    public Task<FileAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.FileAssets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<FileAsset>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
        => await dbContext.FileAssets
            .Where(file => ids.Contains(file.Id))
            .ToListAsync(cancellationToken);

    public async Task AddAsync(FileAsset fileAsset, CancellationToken cancellationToken)
        => await dbContext.FileAssets.AddAsync(fileAsset, cancellationToken);

    public async Task<bool> IsReferencedAsync(Guid fileId, CancellationToken cancellationToken)
    {
        if (await dbContext.Tests.AnyAsync(item => item.CoverImageId == fileId, cancellationToken)
            || await dbContext.Exams.AnyAsync(item => item.CoverImageId == fileId, cancellationToken)
            || await dbContext.Questions.AnyAsync(item => item.ImageId == fileId, cancellationToken))
        {
            return true;
        }

        // Answer definitions are stored as JSON, therefore check their image references after materialization.
        var definitions = await dbContext.Questions
            .AsNoTracking()
            .Select(item => item.AnswerDefinition)
            .ToListAsync(cancellationToken);

        return definitions.Any(definition => definition switch
        {
            TestPlatform.Core.Questions.AnswerDefinition.ChoiceAnswerDefinition choice =>
                choice.Options.Any(option => option.ImageId == fileId),
            TestPlatform.Core.Questions.AnswerDefinition.MatchingAnswerDefinition matching =>
                matching.LeftItems.Any(item => item.ImageId == fileId)
                || matching.RightItems.Any(item => item.ImageId == fileId),
            _ => false,
        });
    }
}
