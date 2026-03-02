using Microsoft.EntityFrameworkCore;
using TestPlatform.Application.Tags;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Infrastructure.Postgres.Tags;

public class TagsReadRepository : ITagsReadRepository
{
    private readonly TestPlatformDbContext _context;

    public TagsReadRepository(TestPlatformDbContext context) => _context = context;

    public Task<TagResponse?> ReadTagByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Tags
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TagResponse(t.Id, t.Name, t.Description))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<List<TagResponse>> ReadAllTagsAsync(CancellationToken cancellationToken)
        => _context.Tags
            .AsNoTracking()
            .Select(t => new TagResponse(t.Id, t.Name, t.Description))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Guid>> GetExistingIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
        => await _context.Tags
            .AsNoTracking()
            .Where(t => ids.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

}