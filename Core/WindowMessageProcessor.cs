using Core.Interfaces;
using Core.Interfaces.Infrastructure;
using Core.Interfaces.Monitor;
using Core.Interfaces.Observer;
using Microsoft.Extensions.Logging;
using static Core.Native.NativeMethods;

namespace Core;

public class WindowMessageProcessor : IWindowMessageProcessor
{
    private readonly ILogger<WindowMessageProcessor> _logger;
    private readonly IThreadSyncEvent _threadSyncEvent;
    private readonly IWindowMonitor _windowMonitor;
    private readonly IWindowObserver _windowObserver;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Thread? _messageLoopThread;

    public WindowMessageProcessor(
        ILogger<WindowMessageProcessor> logger,
        IThreadSyncEvent threadSyncEvent,
        IWindowMonitor windowMonitor,
        IWindowObserver windowObserver)
    {
        _logger = logger;
        _threadSyncEvent = threadSyncEvent;
        _windowMonitor = windowMonitor;
        _windowObserver = windowObserver;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void Start()
    {
        _messageLoopThread = new Thread(RunMessageLoop);
        _messageLoopThread.SetApartmentState(ApartmentState.STA);
        _messageLoopThread.Start();
    }

    private void RunMessageLoop()
    {
        try
        {
            _logger.LogInformation("Starting message processing loop");
            InitializeComponents();

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _windowMonitor.ProcessWindowInfoQueue();

                ProcessWindowMessages();

                _threadSyncEvent.WaitWithTimeout();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in message processing loop");
            throw;
        }
        finally
        {
            _logger.LogInformation("Message processing loop stopped");
        }
    }

    private void InitializeComponents()
    {
        _threadSyncEvent.Initialization();

        if (!_windowObserver.Initialize())
        {
            _logger.LogError("Failed to initialize window observer");
            _cancellationTokenSource.Cancel();
            return;
        }

        if (!_windowMonitor.Initialize())
        {
            _logger.LogError("Failed to initialize window monitor");
            _cancellationTokenSource.Cancel();
        }
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
        _cancellationTokenSource.Cancel();
        _messageLoopThread?.Join();
    }
}