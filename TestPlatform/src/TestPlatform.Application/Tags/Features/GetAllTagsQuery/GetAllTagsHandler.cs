using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Application.Tags.Features.GetAllTagsQuery;

public record GetAllTagsQuery() : IQuery;

public class GetAllTagsHandler : IQueryHandler<List<TagResponse>, GetAllTagsQuery>
{
    private readonly IReadTagsRepository _readTagsRepository;
    private readonly ILogger<GetAllTagsHandler> _logger;

    public GetAllTagsHandler(IReadTagsRepository readTagsRepository, ILogger<GetAllTagsHandler> logger)
    {
        _readTagsRepository = readTagsRepository;
        _logger = logger;
    }

    public async Task<List<TagResponse>?> Handle(GetAllTagsQuery query, CancellationToken cancellationToken)
    {
        var tags = await _readTagsRepository.ReadAllTagsAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} tags", tags.Count);

        return tags;
    }
}
