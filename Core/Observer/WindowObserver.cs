using Core.Interfaces.Observer;
using Core.Native.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Native.NativeMethods;

namespace Core.Observer;

public class WindowObserver : IWindowObserver
{
    private WindowObserverMessageHandlerDelegate? _proc;
    private nint _hookId = nint.Zero;
    private readonly IWindowObserverCallback _windowObserverCallback;

    public WindowObserver(IWindowObserverCallback windowObserverCallback)
    {
        _windowObserverCallback = windowObserverCallback;
    }

    public bool Initialize() 
    {
        _hookId = GetEventHook();
        return _hookId != nint.Zero;
    }


    private nint GetEventHook()
    {
        _proc = _windowObserverCallback.HandleWindowEvent;
        return SetWinEventHook(
            WindowEvent.EVENT_SYSTEM_FOREGROUND,
            WindowEvent.EVENT_OBJECT_LOCATIONCHANGE,
            nint.Zero,
            _proc,
            0,
            0,
            WinEventHookFlags.WINEVENT_OUTOFCONTEXT);
    }

    public void Dispose()
    {
        if (_proc == null || _hookId == nint.Zero) 
        {
            return;
        }

        UnhookWinEvent(_hookId);
    }
}
