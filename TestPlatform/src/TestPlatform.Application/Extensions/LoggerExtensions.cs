using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace TestPlatform.Application.Extensions;

public static class LoggerExtensions
{
    public static void LogResult(this ILogger logger, string action, Guid? id, Result result)
    {
        if (result.IsSuccess)
            logger.LogInformation("{Action} succeeded{Id}", action, id.HasValue ? $" for id {id}" : "");
        else
            logger.LogWarning("{Action} failed{Id}: {Error}", action, id.HasValue ? $" for id {id}" : "", result.Error);
    }

    public static void LogResult<T>(this ILogger logger, string action, Guid? id, Result<T> result)
    {
        if (result.IsSuccess)
            logger.LogInformation("{Action} succeeded{Id}", action, id.HasValue ? $" for id {id}" : "");
        else
            logger.LogWarning("{Action} failed{Id}: {Error}", action, id.HasValue ? $" for id {id}" : "", result.Error);
    }
}