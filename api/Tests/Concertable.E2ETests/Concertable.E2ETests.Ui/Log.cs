using Microsoft.Extensions.Logging;

namespace Concertable.E2ETests.Ui.Support;

internal static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Playwright trace saved to playwright-traces/")]
    internal static partial void PlaywrightTraceSaved(this ILogger logger);
}
