using Core.Common;
using Core.Entities;
using Core.Interfaces.Infrastructure;
using Core.Interfaces.Monitor;
using Core.Native.Enums;
using Microsoft.Extensions.Logging;
using static Core.Native.NativeMethods;

namespace Core.Monitor;

public delegate nint WindowMonitorMessageHandlerDelegate(nint windowHandle, WindowMessages message, nint wParam, nint lParam);

public class WindowMonitorCallback : IWindowMonitorCallback
{
    private const int REFRESH_TIMER_ID = 1;
    private const int REFRESH_INTERVAL_MS = 100;
    private const int TRANSPARENT_MODE = 1;
    private const int TEXT_MARGIN_LEFT = 5;
    private const int LINE_HEIGHT = 20;
    private const int INITIAL_Y_POSITION = 5;

    private readonly IWindowInfoManager _windowInfoManager;
    private readonly ILogger<WindowMonitorCallback> _logger;

    public WindowMonitorCallback(
        IWindowInfoManager windowInfoManager,
        ILogger<WindowMonitorCallback> logger)
    {
        _windowInfoManager = windowInfoManager;
        _logger = logger;
    }

    public nint OnWindowMessage(nint windowHandle, WindowMessages message, nint wParam, nint lParam)
    {
        switch (message)
        {
            case WindowMessages.WM_CREATE:
                InitializeRefreshTimer(windowHandle);
                break;
            case WindowMessages.WM_PAINT:
                RenderWindowInformation(windowHandle);
                break;
            case WindowMessages.WM_TIMER:
                RefreshWindowContent(windowHandle);
                break;
            case WindowMessages.WM_DESTROY:
                CleanupWindow(windowHandle);
                break;
            default:
                return DefWindowProc(windowHandle, message, wParam, lParam);
        }
        return nint.Zero;
    }

    private void InitializeRefreshTimer(nint windowHandle)
    {
        SetTimer(windowHandle, REFRESH_TIMER_ID, REFRESH_INTERVAL_MS, nint.Zero);
    }

    private void RefreshWindowContent(nint windowHandle)
    {
        InvalidateRect(windowHandle, nint.Zero, true);
    }

    private void CleanupWindow(nint windowHandle)
    {
        DestroyWindow(windowHandle);
    }

    private void RenderWindowInformation(nint windowHandle)
    {
        _windowInfoManager.TryGetValue(windowHandle, out var windowInfo);
        PAINTSTRUCT paintStruct;
        nint deviceContext = BeginPaint(windowHandle, out paintStruct);

        try
        {
            ConfigureGraphicsContext(deviceContext, paintStruct);
            int yPosition = INITIAL_Y_POSITION;

            if (windowInfo == null)
            {
                RenderTextLine(deviceContext, Color.Red, "Window info not found", ref yPosition);
            }
            else
            {
                RenderInformationLabels(deviceContext, windowInfo);
            }

            EndPaint(windowHandle, ref paintStruct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering window information");
        }
    }

    private void ConfigureGraphicsContext(nint deviceContext, PAINTSTRUCT paintStruct)
    {
        var backgroundBrush = CreateSolidBrush(Color.Black);
        FillRect(deviceContext, ref paintStruct.rcPaint, backgroundBrush);
        DeleteObject(backgroundBrush);
        SetBkMode(deviceContext, TRANSPARENT_MODE);
    }

    private void RenderInformationLabels(nint deviceContext, WindowInfo windowInfo)
    {
        int currentY = INITIAL_Y_POSITION;

        RenderLabelValuePair(deviceContext, ref currentY, "Process name",
            windowInfo.SystemProcess?.ToString() ?? "unknown");

        RenderLabelValuePair(deviceContext, ref currentY, "Position", string.Empty);
        RenderMetrics(deviceContext, ref currentY, windowInfo);

        RenderLabelValuePair(deviceContext, ref currentY, "Timer:",
            FormatElapsedTime(windowInfo.CreatedTime));
    }

    private void RenderMetrics(nint deviceContext, ref int yPosition, WindowInfo windowInfo)
    {
        RenderTextLine(deviceContext, Color.White,
            $"Width - {windowInfo.WindowBounds?.Width.ToString() ?? "unknown"}", ref yPosition);
        RenderTextLine(deviceContext, Color.White,
            $"Height - {windowInfo.WindowBounds?.Height.ToString() ?? "unknown"}", ref yPosition);
        RenderTextLine(deviceContext, Color.White,
            $"TopLeftX - {windowInfo.WindowBounds?.TopLeftX.ToString() ?? "unknown"}", ref yPosition);
        RenderTextLine(deviceContext, Color.White,
            $"TopLeftY - {windowInfo.WindowBounds?.TopLeftY.ToString() ?? "unknown"}", ref yPosition);
    }

    private void RenderLabelValuePair(nint deviceContext, ref int yPosition, string label, string value)
    {
        RenderTextLine(deviceContext, Color.Red, label, ref yPosition);
        if (!string.IsNullOrEmpty(value))
        {
            RenderTextLine(deviceContext, Color.White, value, ref yPosition);
        }
    }

    private void RenderTextLine(nint deviceContext, uint color, string text, ref int yPosition)
    {
        SetTextColor(deviceContext, color);
        TextOut(deviceContext, TEXT_MARGIN_LEFT, yPosition, text, text.Length);
        yPosition += LINE_HEIGHT;
    }

    private string FormatElapsedTime(DateTime startTime)
    {
        var elapsedTime = DateTime.UtcNow - startTime;
        return elapsedTime.ToString(@"hh\:mm\:ss\.fff");
    }
}
