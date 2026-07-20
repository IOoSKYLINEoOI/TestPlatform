using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Questions.Tags;
using TestPlatform.Core.Questions;

namespace TestPlatform.Infrastructure.Postgres.Questions;

public class TagsRepository : ITagsRepository
{
    private readonly TestPlatformDbContext _context;

    public TagsRepository(TestPlatformDbContext context) => _context = context;

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _context.Tags.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<bool> ExistsByNameAsync(
        Guid? excludedTagId,
        string name,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToUpperInvariant();

        return await _context.Tags.AnyAsync(
            x => x.Id != excludedTagId && x.Name.ToUpper() == normalizedName,
            cancellationToken);
    }

    public async Task<int> GetUsageCountAsync(Guid tagId, CancellationToken cancellationToken)
        => await _context.Questions.CountAsync(
            question => question.Tags.Any(tag => tag.Id == tagId),
            cancellationToken);

    public async Task<int> MergeAsync(Tag sourceTag, Tag targetTag, CancellationToken cancellationToken)
    {
        var questions = await _context.Questions
            .Include(question => question.Tags)
            .Where(question => question.Tags.Any(tag => tag.Id == sourceTag.Id))
            .ToListAsync(cancellationToken);

        foreach (var question in questions)
        {
            var mergedTags = question.Tags
                .Where(tag => tag.Id != sourceTag.Id)
                .Append(targetTag);

            question.ReplaceTags(mergedTags);
        }

        _context.Tags.Remove(sourceTag);
        return questions.Count;
    }

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken)
        => await _context.Tags.AddAsync(tag, cancellationToken);

    public void Delete(Tag tag)
        => _context.Tags.Remove(tag);
}
