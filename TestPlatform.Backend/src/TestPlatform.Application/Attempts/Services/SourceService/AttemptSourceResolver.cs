using CSharpFunctionalExtensions;
using TestPlatform.Application.Attempts.Interfaces;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Services.SourceService;

public class AttemptSourceResolver
{
    private readonly Dictionary<AttemptType, IAttemptSourceService> _sources;

    public AttemptSourceResolver(IEnumerable<IAttemptSourceService> sources)
    {
        _sources = sources.ToDictionary(x => x.Type);
    }

    public Task<Result<AttemptSource>> GetSourceAsync(
        AttemptType type,
        Guid sourceId,
        CancellationToken ct)
    {
        if (!_sources.TryGetValue(type, out var service))
        {
            return Task.FromResult(Result.Failure<AttemptSource>("attempt.unsupported_type"));
        }

        return service.GetSourceAsync(sourceId, ct);
    }
}
