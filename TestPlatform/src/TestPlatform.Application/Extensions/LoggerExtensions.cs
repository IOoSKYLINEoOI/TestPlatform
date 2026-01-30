using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace TestPlatform.Application.Extensions;

public static class LoggerExtensions
{
    public static void LogResult(this ILogger logger, string action, Guid id, Result result)
    {
        if (result.IsSuccess)
            logger.LogInformation("{Action} succeeded for id {Id}", action, id);
        else
            logger.LogWarning("{Action} failed for id {Id}: {Error}", action, id, result.Error);
    }
}