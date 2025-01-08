using Core.Interfaces;
using Core.Interfaces.Monitor;
using Core.Interfaces.Observer;
using Microsoft.Extensions.Logging;

namespace Core;

public class Tracker : ITracker
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly ILogger<Tracker> _logger;
    private readonly IWindowMonitorThread _windowMessageProcessor;
    private readonly IWindowObserverThread _windowObserverThread;

    public Tracker(
        ILogger<Tracker> logger,
        IWindowMonitorThread windowMessageProcessor,
        IWindowObserverThread windowObserverThread)
    {
        _logger = logger;
        _windowMessageProcessor = windowMessageProcessor;
        _windowObserverThread = windowObserverThread;
    }

    public void Run(string[] args)
    {
        _logger.LogDebug("Start program");
        _windowMessageProcessor.Start(cts.Token);
        _windowObserverThread.Start(cts.Token);
        _logger.LogDebug("Stop program");
    }
}
