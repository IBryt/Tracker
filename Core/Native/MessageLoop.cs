using Core.Interfaces.Native;
using static Core.Native.EventProcessor;
using static Core.Native.NativeMethods;


namespace Core.Native;

public class MessageLoop : IMessageLoop
{
    private nint _hookId = nint.Zero;
    private bool _disposedValue;
    private Thread? _thread;
    private CancellationTokenSource? _cancellationTokenSource;
    private WindowEventProcDelegate? _proc;
    private readonly IEventProcessor _eventProcessor;

    public MessageLoop(IEventProcessor eventProcessor)
    {
        _eventProcessor = eventProcessor;
    }

    public void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        _thread = new Thread(() =>
        {
            _hookId = GetEventHook();

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
        })
        {
            IsBackground = true
        };

        _thread.Start();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            if (_hookId != nint.Zero)
            {
                UnhookWinEvent(_hookId);
                _hookId = nint.Zero;
            }

            if (_thread != null && _thread.IsAlive)
            {
                _thread.Interrupt();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private nint GetEventHook()
    {
        _proc = _eventProcessor.EventProcessorCallback;
        return SetWinEventHook(
            WinEvent.EVENT_SYSTEM_FOREGROUND,
            WinEvent.EVENT_OBJECT_LOCATIONCHANGE,
            IntPtr.Zero,
            _proc,
            0,
            0,
            WinEventHookFlags.WINEVENT_OUTOFCONTEXT);
    }
}
