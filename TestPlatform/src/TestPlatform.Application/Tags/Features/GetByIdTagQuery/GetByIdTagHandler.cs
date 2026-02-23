using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Application.Tags.Features.GetByIdTagQuery;

public record GetByIdTagQuery(Guid Id) : IQuery;

public class GetByIdTagHandler : IQueryHandler<TagResponse, GetByIdTagQuery>
{
    private readonly IReadTagsRepository _readTagsRepository;
    private readonly ILogger<GetByIdTagHandler> _logger;

    public GetByIdTagHandler(IReadTagsRepository readTagsRepository, ILogger<GetByIdTagHandler> logger)
    {
        _readTagsRepository = readTagsRepository;
        _logger = logger;
    }

    public async Task<TagResponse?> Handle(GetByIdTagQuery query, CancellationToken cancellationToken)
    {
        var tag = await _readTagsRepository.ReadTagByIdAsync(query.Id, cancellationToken);

        if (tag == null)
            _logger.LogWarning("Tag with id {Id} not found", query.Id);
        else
            _logger.LogInformation("Get Tag with id {Id}", query.Id);

        return tag;
    }
}
