using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core;

public class Tracker : ITracker
{
    private readonly ILogger<Tracker> _logger;
    private readonly IWindowMessageProcessor _windowMessageProcessor;
    
    public Tracker(ILogger<Tracker> logger, IWindowMessageProcessor windowMessageProcessor)
    {
        _logger = logger;
        _windowMessageProcessor = windowMessageProcessor;
    }

    public void Run(string[] args)
    {
        _logger.LogDebug("Start program");
        _windowMessageProcessor.Start();
        _logger.LogDebug("Stop program");
    }
}
