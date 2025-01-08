using Core.Common;
using Core.Entities;
using Core.Interfaces.Infrastructure;
using Core.Interfaces.Monitor;
using Core.Native.Enums;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using static Core.Native.NativeMethods;

namespace Core.Monitor;

public class WindowMonitorEventHandler : IWindowMonitorEventHandler
{
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const int LWA_ALPHA = 0x00000002;
    private const int DEFAULT_WINDOW_WIDTH = 200;
    private const int DEFAULT_WINDOW_HEIGHT = 200;
    private const int WINDOW_OFFSET = 50;
    private const byte WINDOW_OPACITY = 128;
    private const int WINDOW_BORDER_OFFSET = 8;

    private readonly ILogger<WindowMonitorEventHandler> _logger;
    private readonly IWindowInfoQueue _windowInfoQueue;
    private readonly IWindowInfoManager _windowInfoManager;
    private readonly static Dictionary<nint, nint> _windows = new();

    public WindowMonitorEventHandler(
        ILogger<WindowMonitorEventHandler> logger,
        IWindowInfoQueue windowInfoQueue,
        IWindowInfoManager windowInfoManager)
    {
        _logger = logger;
        _windowInfoQueue = windowInfoQueue;
        _windowInfoManager = windowInfoManager;
    }

    public void HandleWindowEvent(WindowInfo? windowInfo)
    {
        if (windowInfo == null) return;

        try
        {
            ProcessWindowInfo(windowInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing window info for parent window: {ParentHwnd}",
                windowInfo.ParentHwnd);
        }
    }

    private void ProcessWindowInfo(WindowInfo windowInfo)
    {
        switch (windowInfo.WindowEvent)
        {
            case WindowEvent.EVENT_SYSTEM_FOREGROUND:
                HandleForegroundEvent(windowInfo);
                break;
            case WindowEvent.EVENT_OBJECT_LOCATIONCHANGE:
                HandleLocationChangedEvent(windowInfo);
                break;
            case WindowEvent.EVENT_OBJECT_DESTROY:
                HandleDestroyEvent(windowInfo);
                break;
        }
    }

    private void HandleForegroundEvent(WindowInfo windowInfo)
    {
        if (!_windows.ContainsKey(windowInfo.ParentHwnd))
        {
            CreateChildWindow(windowInfo);
            _logger.LogInformation("Created new window for parent handle: {ParentHwnd}",
                windowInfo.ParentHwnd);
        }
    }

    private void HandleLocationChangedEvent(WindowInfo windowInfo)
    {
        if (!_windows.TryGetValue(windowInfo.ParentHwnd, out var childHwnd)) return;

        if (!GetWindowRect(windowInfo.ParentHwnd, out RECT parentRect))
        {
            _logger.LogWarning("Failed to get parent window rectangle for handle: {ParentHwnd}",
                windowInfo.ParentHwnd);
            return;
        }

        UpdateChildWindowPosition(childHwnd, windowInfo, parentRect);
    }

    private void HandleDestroyEvent(WindowInfo windowInfo)
    {
        if (_windows.ContainsKey(windowInfo.ParentHwnd))
        {
            DestroyChildWindow(windowInfo);
            _logger.LogInformation("Destroyed window for parent handle: {ParentHwnd}",
                windowInfo.ParentHwnd);
        }
    }

    private nint CreateChildWindow(WindowInfo windowInfo)
    {
        if (_windows.ContainsKey(windowInfo.ParentHwnd))
        {
            return _windows[windowInfo.ParentHwnd];
        }

        if (!GetWindowRect(windowInfo.ParentHwnd, out RECT parentRect))
        {
            throw new InvalidOperationException("Failed to get parent window rectangle");
        }
        var windowPosition = CalculateWindowPosition(parentRect, windowInfo);

        var childHwnd = CreateWindowEx(
            WindowStylesEx.WS_EX_NOACTIVATE | WindowStylesEx.WS_EX_TRANSPARENT | WindowStylesEx.WS_EX_LAYERED,
            Constants.WindowName,
            null,
            WindowDwStyle.WS_POPUP | WindowDwStyle.WS_VISIBLE,
            windowPosition.X,
            windowPosition.Y,
            windowPosition.Width,
            windowPosition.Height,
            windowInfo.ParentHwnd,
            nint.Zero,
            GetModuleHandle(null),
            nint.Zero
        );

        if (childHwnd == nint.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to create window: {error}");
        }

        SetLayeredWindowAttributes(childHwnd, 0x000000, WINDOW_OPACITY, LWA_ALPHA);
        AddOrUpdateWindowInfo(childHwnd, windowInfo);
        _windows.TryAdd(windowInfo.ParentHwnd, childHwnd);

        return childHwnd;
    }

    private void UpdateChildWindowPosition(nint childHwnd, WindowInfo windowInfo, RECT parentRect)
    {
        var windowPosition = CalculateWindowPosition(parentRect, windowInfo);

        SetWindowPos(childHwnd, nint.Zero,
            windowPosition.X,
            windowPosition.Y,
            windowPosition.Width,
            windowPosition.Height,
            SWP_SHOWWINDOW);

        InvalidateRect(childHwnd, nint.Zero, true);
        AddOrUpdateWindowInfo(childHwnd, windowInfo);

        _logger.LogDebug("Updated position for child window: {ChildHwnd}", childHwnd);
    }

    private WindowPosition CalculateWindowPosition(RECT parentRect, WindowInfo windowInfo)
    {
        if (windowInfo.WindowBounds == null)
        {
            throw new ArgumentNullException(nameof(windowInfo.WindowBounds),
                "Window bounds cannot be null when calculating position");
        }

        return WindowPosition.CreateDefault(
            CalculateXPosition(parentRect, windowInfo.WindowBounds),
            CalculateYPosition(parentRect, windowInfo.WindowBounds)
        );
    }
    private static int CalculateXPosition(RECT parentRect, WindowBounds bounds) =>
      parentRect.Left + bounds.Width - DEFAULT_WINDOW_WIDTH - WINDOW_BORDER_OFFSET;

    private static int CalculateYPosition(RECT parentRect, WindowBounds bounds) =>
       parentRect.Top + bounds.Height - DEFAULT_WINDOW_HEIGHT - WINDOW_BORDER_OFFSET;

    private void AddOrUpdateWindowInfo(nint hwnd, WindowInfo windowInfo)
    {
        _windowInfoManager.AddOrUpdate(hwnd, windowInfo, (key, oldValue) =>
        {
            windowInfo.CreatedTime = oldValue.CreatedTime;
            return windowInfo;
        });
    }

    private void DestroyChildWindow(WindowInfo windowInfo)
    {
        if (_windows.Remove(windowInfo.ParentHwnd, out nint childHwnd) && childHwnd != nint.Zero)
        {
            DestroyWindow(childHwnd);
        }
    }

    private struct WindowPosition()
    {
        public int X { get; init; }
        public int Y { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }

        public static WindowPosition CreateDefault(int x, int y) => new()
        {
            X = x,
            Y = y,
            Width = DEFAULT_WINDOW_WIDTH,
            Height = DEFAULT_WINDOW_HEIGHT
        };
    }
}
