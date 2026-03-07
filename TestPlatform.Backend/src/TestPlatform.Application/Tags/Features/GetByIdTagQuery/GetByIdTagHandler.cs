using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Application.Tags.Features.GetByIdTagQuery;

public record GetByIdTagQuery(Guid Id) : IQuery;

public class GetByIdTagHandler : IQueryHandler<TagResponse, GetByIdTagQuery>
{
    private readonly ITagsReadRepository _tagsReadRepository;
    private readonly ILogger<GetByIdTagHandler> _logger;

    public GetByIdTagHandler(ITagsReadRepository tagsReadRepository, ILogger<GetByIdTagHandler> logger)
    {
        _tagsReadRepository = tagsReadRepository;
        _logger = logger;
    }

    public async Task<TagResponse?> Handle(GetByIdTagQuery query, CancellationToken cancellationToken)
    {
        var tag = await _tagsReadRepository.ReadTagByIdAsync(query.Id, cancellationToken);

        if (tag == null)
            _logger.LogWarning("Tag with id {Id} not found", query.Id);
        else
            _logger.LogInformation("Get Tag with id {Id}", query.Id);

        return tag;
    }
}
