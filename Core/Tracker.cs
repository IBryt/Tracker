using Core.Interfaces;
using Core.Interfaces.Native;
using Microsoft.Extensions.Logging;

namespace Core;

public class Tracker : ITracker
{
    private readonly ILogger<Tracker> _logger;
    private readonly IMessageLoop _messageLoop;
    private readonly IWindowMessageProcessor _windowMessageProcessor;
    
    public Tracker(ILogger<Tracker> logger, IMessageLoop messageLoop,  IWindowMessageProcessor windowMessageProcessor)
    {
        _logger = logger;
        _messageLoop = messageLoop;
        _windowMessageProcessor = windowMessageProcessor;
    }

    public void Run(string[] args)
    {
        _logger.LogDebug("Start program");
        _messageLoop.Start();
        _windowMessageProcessor.Start();
        _logger.LogDebug("Stop program");
    }
}
