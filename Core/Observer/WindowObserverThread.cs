using Core.Interfaces.Observer;
using Microsoft.Extensions.Logging;
using static Core.Native.NativeMethods;

namespace Core.Observer;

public class WindowObserverThread : IWindowObserverThread
{
    private nint _hookId = nint.Zero;
    private Thread? _thread;

    private readonly ILogger<WindowObserverThread> _logger;
    private readonly IWindowObserver _windowObserver;

    public WindowObserverThread(
        ILogger<WindowObserverThread> logger,
        IWindowObserver windowObserver)
    {
        _logger = logger;
        _windowObserver = windowObserver;
    }

    public void Start(CancellationToken token)
    {
        _logger.LogInformation("Starting window observer thread...");

        _thread = new Thread(() => Run(token))
        {
            IsBackground = true
        };
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();

        _logger.LogInformation("Window observer thread started.");
    }

    private void Run(CancellationToken token)
    {
        try
        {
            _logger.LogInformation("Initializing window observer...");

            if (!_windowObserver.Initialize())
            {
                _logger.LogError("Failed to initialize window observer.");
                return;
            }

            _logger.LogInformation("Window observer successfully initialized. Entering message loop.");

            while (!token.IsCancellationRequested)
            {
                var result = GetMessage(out var message, nint.Zero, 0, 0);

                if (result <= 0)
                {
                    continue;
                }

                TranslateMessage(ref message);
                DispatchMessage(ref message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in the window observer thread.");
            throw;
        }
        finally
        {
            _logger.LogInformation("Window observer thread is stopping...");
        }
    }

    public void Dispose()
    {
        UnhookWinEvent(_hookId);
        _thread?.Interrupt();
    }
}