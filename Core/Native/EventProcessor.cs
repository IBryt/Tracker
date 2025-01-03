using Core.Entities;
using Core.Enums;
using Core.Extensions;
using Core.Interfaces;
using Core.Interfaces.Native;
using System.Diagnostics;
using static Core.Native.NativeMethods;

namespace Core.Native;

public class EventProcessor : IEventProcessor
{
    public delegate void WindowEventProcDelegate(nint hWinEventHook, WinEvent eventType, nint hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
    private readonly HashSet<nint> Hwnds = new();
    private static readonly Dictionary<WinEvent, EventType> _eventMapping = new()
    {
        { WinEvent.EVENT_SYSTEM_FOREGROUND, EventType.Foreground },
        { WinEvent.EVENT_SYSTEM_MOVESIZEEND, EventType.MoveSizeEnd },
        { WinEvent.EVENT_SYSTEM_MINIMIZESTART, EventType.MinimizeStart },
        { WinEvent.EVENT_SYSTEM_MINIMIZEEND, EventType.MinimizeEnd },
        { WinEvent.EVENT_OBJECT_DESTROY, EventType.Destroy },
    };
    private readonly IWindowActionQueue _windowActionQueue;

    public EventProcessor(IWindowActionQueue windowActionQueue)
    {
        _windowActionQueue = windowActionQueue;
    }

    public void EventProcessorCallback(nint hWinEventHook, WinEvent eventType, nint hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        if (hwnd == nint.Zero || !_eventMapping.TryGetValue(eventType, out var mappedEventType))
        {
            return;
        }

        if (mappedEventType == EventType.Foreground)
        {
            HandleWindowForeground(hwnd, mappedEventType);
        }
        else if (mappedEventType == EventType.Destroy)
        {
            HandleDestroyEvent(hwnd);
        }
        else
        {
            HandleWindowAction(hwnd, mappedEventType);
        }
    }

    private void HandleWindowForeground(nint hwnd, EventType eventType)
    {
        using var process = GetProcess(hwnd);
        if (TryGetSystemProcess(hwnd, process, out var systemProcess) && !Hwnds.Contains(hwnd))
        {
            var windowBounds = GetWindowBounds(hwnd, process);
            var windowAction = new WindowAction
            {
                Hwnd = hwnd,
                EventType = eventType,
                WindowBounds = windowBounds,
            };

            Hwnds.Add(hwnd);
            _windowActionQueue.Add(windowAction);
        }
    }

    private void HandleWindowAction(nint hwnd, EventType eventType)
    {
        using var process = GetProcess(hwnd);
        if (TryGetSystemProcess(hwnd, process, out var systemProcess))
        {
            var windowBounds = GetWindowBounds(hwnd, process);
            var windowAction = new WindowAction
            {
                Hwnd = hwnd,
                EventType = eventType,
                WindowBounds = windowBounds,
            };

            _windowActionQueue.Add(windowAction);
        }
    }

    private void HandleDestroyEvent(nint hwnd)
    {
        using var process = GetProcess(hwnd);
        if (TryGetSystemProcess(hwnd, process, out var systemProcess) && Hwnds.Contains(hwnd))
        {
            var windowAction = new WindowAction
            {
                Hwnd = hwnd,
                EventType = EventType.Destroy,
            };
            Hwnds.Remove(hwnd);
            _windowActionQueue.Add(windowAction);
        }
    }

    private bool TryGetSystemProcess(nint hwnd, Process process, out SystemProcesses? systemProcess)
    {
        systemProcess = process.ProcessName.GetEnumValueOrNull<SystemProcesses>();
        return systemProcess != null;
    }

    private Process GetProcess(nint hwnd)
    {
        GetWindowThreadProcessId(hwnd, out uint processId);
        return Process.GetProcessById((int)processId);
    }

    private WindowBounds? GetWindowBounds(nint hwnd, Process process)
    {
        WindowBounds? windowBounds = null;
        RECT rect;
        if (GetWindowRect(hwnd, out rect))
        {
            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            windowBounds = new WindowBounds
            {
                Height = height,
                Width = width,
                TopLeftX = rect.Left,
                TopLeftY = rect.Top,
            };
        }

        return windowBounds;
    }
}
