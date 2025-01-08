using Core.Common;
using Core.Interfaces.Infrastructure;
using Core.Interfaces.Monitor;
using Core.Native.Enums;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using static Core.Native.NativeMethods;

namespace Core.Monitor;

public class WindowMonitor : IWindowMonitor
{
    private const uint COLOR_WINDOW = 5;
    private WindowMonitorMessageHandlerDelegate? _proc;

    private readonly ILogger<WindowMonitor> _logger;
    private readonly IWindowMonitorCallback _windowMonitorCallback;
    private readonly IWindowInfoQueue _windowInfoQueue;
    private readonly IWindowMonitorEventHandler _windowMonitorEventHandler;

    public WindowMonitor(
        ILogger<WindowMonitor> logger,
        IWindowMonitorCallback windowMonitorCallback,
        IWindowInfoQueue windowInfoQueue,
        IWindowMonitorEventHandler windowMonitorEventHandler)
    {
        _logger = logger;
        _windowMonitorCallback = windowMonitorCallback;
        _windowInfoQueue = windowInfoQueue;
        _windowMonitorEventHandler = windowMonitorEventHandler;
    }

    public bool Initialize()
    {
        if (!RegisterWindowClass())
        {
            _logger.LogError($"Window class registration error: {Marshal.GetLastWin32Error()}");
            return false;
        }
        return true;
    }

    public void ProcessWindowInfoQueue()
    {
        while (_windowInfoQueue.TryDequeue(out var windowInfo))
        {
            _windowMonitorEventHandler.HandleWindowEvent(windowInfo);
        }
    }

    private bool RegisterWindowClass()
    {
        _proc = _windowMonitorCallback.OnWindowMessage;

        var wndClass = new WNDCLASSEX
        {
            cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX)),
            style = WindowClassStyles.CS_HREDRAW | WindowClassStyles.CS_VREDRAW,
            lpfnWndProc = _proc,
            cbClsExtra = 0,
            cbWndExtra = 0,
            hInstance = GetModuleHandle(null),
            hIcon = nint.Zero,
            hCursor = nint.Zero,
            hbrBackground = (nint)(COLOR_WINDOW + 1),
            lpszMenuName = null,
            lpszClassName = Constants.WindowName,
            hIconSm = nint.Zero
        };

        return RegisterClassEx(ref wndClass);
    }
}