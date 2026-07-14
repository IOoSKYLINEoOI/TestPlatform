using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Tags;
using TestPlatform.Core.Questions;

namespace TestPlatform.Infrastructure.Postgres.Questions;

public class TagsRepository : ITagsRepository
{
    private readonly TestPlatformDbContext _context;

    public TagsRepository(TestPlatformDbContext context) => _context = context;

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _context.Tags.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken)
        => await _context.Tags.AddAsync(tag, cancellationToken);

    public void Delete(Tag tag)
        => _context.Tags.Remove(tag);
}