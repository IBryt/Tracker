using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.Native;
using System.Collections.Concurrent;

namespace Core;

public class WindowHandler : IWindowHandler
{
    private bool _disposedValue;
    private Thread? _thread;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly ConcurrentDictionary<nint, WindowAction> _activeWindows = new();
    private readonly IWindowActionQueue _windowActionQueue;
    private readonly IWindowManager _windowManager;
    public WindowHandler(IWindowActionQueue windowActionQueue, IWindowManager windowManager)
    {
        _windowActionQueue = windowActionQueue;
        _windowManager = windowManager;
    }

    public void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        _thread = new Thread(async () =>
        {
            while (true)
            {
                while (!_windowActionQueue.IsEmpty())
                {
                    HandleAction();

                }
                await Task.Delay(100);
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

    private void HandleAction() 
    {
        var action = _windowActionQueue.Take();
        var isWindowActive = _activeWindows.ContainsKey(action.Hwnd);

        if (isWindowActive)
        {
            _windowManager.RemoveWindow(action.Hwnd);
        }
        
        if (action.EventType == Enums.EventType.MinimizeStart  || action.EventType == Enums.EventType.Destroy)
        {
            _windowManager.RemoveWindow(action.Hwnd);
            _activeWindows.TryRemove(action.Hwnd, out _);
            return;
        }

        if (!isWindowActive)
        {
            _activeWindows.TryAdd(action.Hwnd, action);
        }

        _windowManager.AddWindow(action);
    }
}
