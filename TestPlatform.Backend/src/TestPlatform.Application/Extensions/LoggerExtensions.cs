using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace TestPlatform.Application.Extensions;

public static class LoggerExtensions
{
    public static void LogResult(this ILogger logger, string action, Guid? id, Result result)
    {
        if (result.IsSuccess)
        {
            logger.LogInformation("{Operation} succeeded for {EntityId}", action, id);
        }
        else
        {
            logger.LogWarning("{Operation} failed for {EntityId}: {ErrorCode}", action, id, result.Error);
        }
    }

    public static void LogResult<T>(this ILogger logger, string action, Guid? id, Result<T> result)
    {
        if (result.IsSuccess)
        {
            logger.LogInformation("{Operation} succeeded for {EntityId}", action, id);
        }
        else
        {
            logger.LogWarning("{Operation} failed for {EntityId}: {ErrorCode}", action, id, result.Error);
        }
    }
}
