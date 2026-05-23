using Microsoft.Extensions.Logging;
using System.Net;

namespace Concertable.E2ETests;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Condition check failed")]
    internal static partial void ConditionCheckFailed(this ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Polling action failed")]
    internal static partial void PollingActionFailed(this ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Information, Message = "Initializing E2E test fixture")]
    internal static partial void InitializingE2ETestFixture(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "E2E test fixture ready")]
    internal static partial void E2ETestFixtureReady(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Waiting for app to become healthy at {Url}/health")]
    internal static partial void WaitingForAppToBeHealthy(this ILogger logger, string url);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Health check: {StatusCode}")]
    internal static partial void HealthCheck(this ILogger logger, HttpStatusCode statusCode);

    [LoggerMessage(Level = LogLevel.Information, Message = "App is healthy")]
    internal static partial void AppIsHealthy(this ILogger logger);
}
