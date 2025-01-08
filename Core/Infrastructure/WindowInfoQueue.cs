using Core.Entities;
using Core.Interfaces.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Core.Infrastructure;

public class WindowInfoQueue : IWindowInfoQueue
{
    private readonly ILogger _logger;

    private readonly ConcurrentQueue<WindowInfo> windowInfos = new();

    public WindowInfoQueue(ILogger<WindowInfoQueue> logger)
    {
        _logger = logger;
    }

    public void Add(WindowInfo windowInfo)
    {
        windowInfos.Enqueue(windowInfo);
    }

    public bool TryDequeue(out WindowInfo? windowInfo)
    {
        return windowInfos.TryDequeue(out windowInfo);
    }
}
