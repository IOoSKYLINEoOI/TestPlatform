using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using TestPlatform.Application.Abstractions;
using TestPlatform.Contracts.Tags.DTOs;

namespace TestPlatform.Application.Tags.Features.GetAllTagsQuery;

public record GetAllTagsQuery() : IQuery;

public class GetAllTagsHandler : IQueryHandler<IReadOnlyList<TagResponse>, GetAllTagsQuery>
{
    private readonly ITagsReadRepository _tagsReadRepository;
    private readonly ILogger<GetAllTagsHandler> _logger;

    public GetAllTagsHandler(ITagsReadRepository tagsReadRepository, ILogger<GetAllTagsHandler> logger)
    {
        _tagsReadRepository = tagsReadRepository;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<TagResponse>>> Handle(GetAllTagsQuery query, CancellationToken cancellationToken)
    {
        var tags = await _tagsReadRepository.ReadAllTagsAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} tags", tags.Count);

        return Result.Success((IReadOnlyList<TagResponse>)tags);
    }
}
