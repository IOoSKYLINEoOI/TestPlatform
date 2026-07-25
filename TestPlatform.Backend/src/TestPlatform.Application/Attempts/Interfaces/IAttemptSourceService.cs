using CSharpFunctionalExtensions;
using TestPlatform.Application.Attempts.Services.SourceService;
using TestPlatform.Core.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Interfaces;

public interface IAttemptSourceService
{
    AttemptType Type { get; }

    Task<Result<AttemptSource>> GetSourceAsync(
        Guid sourceId,
        CancellationToken cancellationToken);
}
