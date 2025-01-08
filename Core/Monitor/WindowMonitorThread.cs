using Core.Interfaces.Infrastructure;
using Core.Interfaces.Monitor;
using Microsoft.Extensions.Logging;
using static Core.Native.NativeMethods;

namespace Core.Monitor;

public class WindowMonitorThread : IWindowMonitorThread
{
    private readonly ILogger<WindowMonitorThread> _logger;
    private readonly IThreadSyncEvent _threadSyncEvent;
    private readonly IWindowMonitor _windowMonitor;

    private Thread? _thread;

    public WindowMonitorThread(
        ILogger<WindowMonitorThread> logger,
        IThreadSyncEvent threadSyncEvent,
        IWindowMonitor windowMonitor)
    {
        _logger = logger;
        _threadSyncEvent = threadSyncEvent;
        _windowMonitor = windowMonitor;
    }

    public void Start(CancellationToken token)
    {
        _thread = new Thread(() => Run(token));
        _thread.IsBackground = true;
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
    }

    private void Run(CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Window monitor thread started.");

            if (!IsInitializeComponents())
            {
                _logger.LogError("Failed to initialize components. Thread is stopping.");
                return;
            }

            _logger.LogInformation("Entering message processing loop.");

            while (!token.IsCancellationRequested)
            {
                _windowMonitor.ProcessWindowInfoQueue();
                ProcessWindowMessages();
                _threadSyncEvent.WaitWithTimeout();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in the window monitor thread.");
            throw;
        }
        finally
        {
            _logger.LogInformation("Window monitor thread stopped.");
        }
    }

    private bool IsInitializeComponents()
    {
        _threadSyncEvent.Initialization();

        if (!_windowMonitor.Initialize())
        {
            _logger.LogError("Window monitor initialization failed.");
            return false;
        }

        _logger.LogInformation("Window monitor successfully initialized.");
        return true;
    }

    private void ProcessWindowMessages()
    {
        while (PeekMessage(out MSG msg, nint.Zero, 0, 0, 1))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
        }
    }

    public void Stop()
    {
        _thread?.Join();
    }
}