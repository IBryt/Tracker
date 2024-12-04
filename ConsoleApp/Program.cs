using Microsoft.Extensions.Logging;
using Shared;


using var factory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddConsole();
});

var logger = factory.CreateLogger<ProcessTracker>();

logger.LogDebug("Tracking processes...");

CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
CancellationToken token = cancellationTokenSource.Token;

using ProcessTracker processTracker = new(logger);

processTracker.ProcessStarted += (sender, process) =>
{
    logger.LogDebug("Test");
};

await processTracker.StartAsync(token);