using CSharpFunctionalExtensions;
using TestPlatform.Contracts.Attempts.Enums;

namespace TestPlatform.Application.Attempts.Interfaces;

public interface IAttemptSourceService
{
    Task<Result<IAttemptSource>> GetSourceAsync(AttemptTypeDto type, Guid sourceId, CancellationToken cancellationToken);
}