using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace Core;

public class WindowActionQueue : IWindowActionQueue
{
    private readonly BlockingCollection<WindowAction> _actionQueue =
        new BlockingCollection<WindowAction>(new ConcurrentQueue<WindowAction>());
    private readonly ILogger _logger;

    public WindowActionQueue(ILogger<IWindowActionQueue> logger)
    {
        _logger = logger;
    }
    
    public void Add(WindowAction action)
    {
        _logger.LogDebug($"Add - {action.ToString()}");
        _actionQueue.Add(action);
    }

    public WindowAction Take()
    {
        var action = _actionQueue.Take();
        _logger.LogDebug($"Take - {action.ToString()}");
        return action;
    }

    public bool IsEmpty()
    {
        return _actionQueue.Any();
    }
}
