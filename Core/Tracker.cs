using Core.Interfaces;
using Core.Interfaces.Native;
using Microsoft.Extensions.Logging;

namespace Core;

public class Tracker : ITracker
{
    private readonly ILogger<ITracker> _logger;
    private readonly IMessageLoop _messageLoop;
    private readonly IWindowHandler _windowHandler;

    public Tracker(ILogger<ITracker> logger, IMessageLoop messageLoop, IWindowHandler windowHandler)
    {
        _logger = logger;
        _messageLoop = messageLoop;
        _windowHandler = windowHandler;
    }

    public void Run(string[] args)
    {
        _logger.LogDebug("Start");
        _messageLoop.Start();
        _windowHandler.Start();
    }
}
