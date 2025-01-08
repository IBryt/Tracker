using Core.Entities;
using Core.Enums;
using Core.Extensions;
using Core.Interfaces.Infrastructure;
using Core.Interfaces.Observer;
using Core.Native.Enums;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using static Core.Native.NativeMethods;

namespace Core.Observer;

public delegate void WindowObserverMessageHandlerDelegate(nint hWinEventHook, WindowEvent eventType, nint hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

public class WindowObserverCallback : IWindowObserverCallback
{
    private const uint GW_OWNER = 4;
    private const uint GA_ROOT = 2;

    private readonly HashSet<nint> _activeWindows = new();
    private readonly ILogger<WindowObserverCallback> _logger;
    private readonly IWindowInfoQueue _windowInfoQueue;
    private readonly IThreadSyncEvent _threadSyncEvent;

    public WindowObserverCallback(
        ILogger<WindowObserverCallback> logger,
        IWindowInfoQueue windowInfoQueue,
        IThreadSyncEvent threadSyncEvent)
    {
        _logger = logger;
        _windowInfoQueue = windowInfoQueue;
        _threadSyncEvent = threadSyncEvent;
    }

    public void HandleWindowEvent(nint hWinEventHook, WindowEvent eventType, nint hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        try
        {
            if (!IsValidWindow(hwnd, out nint parentHwnd))
            {
                return;
            }

            switch (eventType)
            {
                case WindowEvent.EVENT_SYSTEM_FOREGROUND:
                    HandleForegroundEvent(hwnd);
                    break;

                case WindowEvent.EVENT_OBJECT_DESTROY:
                    HandleDestroyEvent(hwnd);
                    break;

                case WindowEvent.EVENT_OBJECT_LOCATIONCHANGE:
                    HandleLocationChangeEvent(hwnd);
                    break;
            }

            _threadSyncEvent.Reset();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing window event: {ex}");
        }
    }

    private bool IsValidWindow(nint hwnd, out nint parentHwnd)
    {
        parentHwnd = nint.Zero;

        if (hwnd == nint.Zero)
        {
            return false;
        }

        parentHwnd = GetAncestor(hwnd, GA_ROOT);
        if (parentHwnd == nint.Zero)
        {
            return false;
        }

        if (!IsMainWindowByOwner(hwnd))
        {
            return false;
        }

        return hwnd == parentHwnd;
    }

    private void HandleForegroundEvent(nint parentHwnd)
    {
        if (_activeWindows.Contains(parentHwnd))
        {
            return;
        }

        using var process = GetProcess(parentHwnd);
        if (!TryGetSystemProcess(process, out var systemProcess))
        {
            return;
        }

        _activeWindows.Add(parentHwnd);
        AddWindowInfo(parentHwnd, WindowEvent.EVENT_SYSTEM_FOREGROUND, process);
        _logger.LogDebug($"Window activated: {parentHwnd}");
    }

    private void HandleDestroyEvent(nint parentHwnd)
    {
        if (!_activeWindows.Remove(parentHwnd))
        {
            return;
        }

        var windowInfo = new WindowInfo
        {
            ParentHwnd = parentHwnd,
            WindowEvent = WindowEvent.EVENT_OBJECT_DESTROY
        };

        _windowInfoQueue.Add(windowInfo);
        _logger.LogDebug($"Window destroyed: {parentHwnd}");
    }

    private void HandleLocationChangeEvent(nint parentHwnd)
    {
        if (!_activeWindows.Contains(parentHwnd))
        {
            return;
        }

        using var process = GetProcess(parentHwnd);
        if (!TryGetSystemProcess(process, out var systemProcess))
        {
            return;
        }

        AddWindowInfo(parentHwnd, WindowEvent.EVENT_OBJECT_LOCATIONCHANGE, process);
    }

    private void AddWindowInfo(nint hwnd, WindowEvent eventType, Process process)
    {
        var windowInfo = new WindowInfo
        {
            ParentHwnd = hwnd,
            WindowEvent = eventType,
            WindowBounds = GetWindowBounds(hwnd),
            SystemProcess = process.ProcessName.GetEnumValueOrNull<SystemProcesses>()
        };

        _windowInfoQueue.Add(windowInfo);
    }

    private WindowBounds? GetWindowBounds(nint hwnd)
    {
        if (!GetWindowRect(hwnd, out RECT rect))
        {
            return null;
        }

        return new WindowBounds
        {
            Height = rect.Bottom - rect.Top,
            Width = rect.Right - rect.Left,
            TopLeftX = rect.Left,
            TopLeftY = rect.Top,
        };
    }

    private bool IsMainWindowByOwner(nint hwnd) =>
        GetWindow(hwnd, GW_OWNER) == IntPtr.Zero;

    private Process GetProcess(nint hwnd)
    {
        GetWindowThreadProcessId(hwnd, out uint processId);
        return Process.GetProcessById((int)processId);
    }

    private bool TryGetSystemProcess(Process process, out SystemProcesses? systemProcess)
    {
        systemProcess = process.ProcessName.GetEnumValueOrNull<SystemProcesses>();
        return systemProcess != null;
    }
}