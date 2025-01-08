using Core.Common;
using Core.Entities;
using Core.Enums;
using Core.Extensions;
using Core.Interfaces.Infrastructure;
using Core.Interfaces.Native;
using Core.Native.Enums;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static Core.Native.NativeMethods;

namespace Core.Native;

public class EventProcessor : IEventProcessor
{
    public delegate void WindowEventProcDelegate(nint hWinEventHook, Enums.WindowEvent eventType, nint hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    private readonly Dictionary<nint, bool> _activeWindows = new();
    private readonly IWindowInfoQueue _windowInfoQueue;
    private readonly ILogger<EventProcessor> _logger;
    private readonly IThreadSyncEvent _threadSyncEvent;
    //private readonly IWindowInfoManager _windowInfoManager;

    public EventProcessor(IWindowInfoQueue windowInfoQueue, ILogger<EventProcessor> logger, IThreadSyncEvent threadSyncEvent, IWindowInfoManager windowInfoManager)
    {
        _windowInfoQueue = windowInfoQueue;
        _logger = logger;
        _threadSyncEvent = threadSyncEvent;
        //_windowInfoManager = windowInfoManager;
    }

    private void TryGet(WindowInfo windowInfo)
    {
        //_windowInfoManager.TryGetValue(windowInfo.ParentHwnd, out var existing);
        //if (existing != null)
        //{
        //    windowInfo.CreatedTime = existing.CreatedTime;
        //}
    }

    public void EventProcessorCallback(nint hWinEventHook, WindowEvent eventType, nint hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
    {
        try
        {
            if (hwnd == nint.Zero)
            {
                return;
            }

            nint parentHwnd = GetAncestor(hwnd, GA_ROOT);
            if (parentHwnd == nint.Zero && parentHwnd == hwnd)
            {
                return;
            }

            if (!IsThickFrameWindow(hwnd))
            {
                return;
            }

            using var process = GetProcess(parentHwnd);
            if (!TryGetSystemProcess(parentHwnd, process, out var systemProcess))
            {
                return;
            }

            // Обрабатываем только события для главного окна
            if (hwnd != parentHwnd)
            {
                return;
            }

            switch (eventType)
            {
                case WindowEvent.EVENT_SYSTEM_FOREGROUND:
                    // Создаем окно только если оно еще не активно
                    if (!_activeWindows.ContainsKey(parentHwnd))
                    {
                        _activeWindows[parentHwnd] = true;
                        HandleWindowForeground(parentHwnd, process);
                        _logger.LogDebug($"Window activated: {parentHwnd}");
                        _threadSyncEvent.Reset();
                    }
                    break;

                case WindowEvent.EVENT_OBJECT_DESTROY:
                    // Уничтожаем окно только если оно было активно
                    if (_activeWindows.Remove(parentHwnd))
                    {
                        HandleDestroyEvent(parentHwnd, process);
                        _logger.LogDebug($"Window destroyed: {parentHwnd}");
                        _threadSyncEvent.Reset();
                    }
                    break;
                case WindowEvent.EVENT_OBJECT_LOCATIONCHANGE:
                //case EventType.MoveSizeEnd:
                //case EventType.MinimizeStart:
                //case EventType.MinimizeEnd:
                    if (_activeWindows.ContainsKey(parentHwnd))
                    {
                        HandlewindowInfo(parentHwnd, process, eventType);
                        _threadSyncEvent.Reset();
                    }
                    break;
                //case EventType.LocationChanged:
                //    // Обрабатываем событие развертывания окна на весь экран
                //    if (_activeWindows.ContainsKey(parentHwnd))
                //    {
                //        HandleMaximizeEvent(parentHwnd, process);
                //        _logger.LogDebug($"Window maximized: {parentHwnd}");
                //    }
                //    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing window event: {ex}");
        }
    }


    private void HandleMaximizeEvent(nint hwnd, Process process)
    {
        var windowBounds = GetWindowBounds(hwnd);
        var windowInfo = new WindowInfo
        {
            ParentHwnd = hwnd,
            WindowEvent = WindowEvent.EVENT_OBJECT_LOCATIONCHANGE,
            WindowBounds = windowBounds,
            SystemProcess = process.ProcessName.GetEnumValueOrNull<SystemProcesses>()
        };

        _windowInfoQueue.Add(windowInfo);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern nint GetClassLongPtr(nint hWnd, int nIndex);

    // P/Invoke for GetClassName (to retrieve the class name string)
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetClassName(nint hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

    // Constants
    private const int GCW_ATOM = -32; // GCW_ATOM is the index for getting the window class atom
    private const string WC_DIALOG = "#32770";



    static string GetClassNameByAtom(nint classAtom)
    {
        // In real usage, you would need to translate the atom into a class name or compare directly.
        // We use a hardcoded check for simplicity here since WC_DIALOG is usually mapped to the atom "#32770".
        return classAtom.ToString(); // Convert to string, or use specific atom mapping if needed.
    }
    private void HandleWindowForeground(nint hwnd, Process process)
    {
        var windowBounds = GetWindowBounds(hwnd);
        var windowInfo = new WindowInfo
        {
            ParentHwnd = hwnd,
            WindowEvent = WindowEvent.EVENT_SYSTEM_FOREGROUND,
            WindowBounds = windowBounds,
            SystemProcess = process.ProcessName.GetEnumValueOrNull<SystemProcesses>()
        };

        TryGet(windowInfo);
        _windowInfoQueue.Add(windowInfo);
    }

    private void HandlewindowInfo(nint hwnd, Process process, WindowEvent eventType)
    {
        var windowBounds = GetWindowBounds(hwnd);
        var windowInfo = new WindowInfo
        {
            ParentHwnd = hwnd,
            WindowEvent = eventType,
            WindowBounds = windowBounds,
            SystemProcess = process.ProcessName.GetEnumValueOrNull<SystemProcesses>()
        };
        TryGet(windowInfo);
        _windowInfoQueue.Add(windowInfo);
    }

    private void HandleDestroyEvent(nint hwnd, Process process)
    {
        var windowInfo = new WindowInfo
        {
            ParentHwnd = hwnd,
            WindowEvent = WindowEvent.EVENT_OBJECT_DESTROY,
            SystemProcess = process.ProcessName.GetEnumValueOrNull<SystemProcesses>()
        };

        TryGet(windowInfo);
        _windowInfoQueue.Add(windowInfo);
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

    private WindowBounds? GetWindowBounds(nint hwnd)
    {
        WindowBounds? windowBounds = null;
        if (GetWindowRect(hwnd, out RECT rect))
        {
            windowBounds = new WindowBounds
            {
                Height = rect.Bottom - rect.Top,
                Width = rect.Right - rect.Left,
                TopLeftX = rect.Left,
                TopLeftY = rect.Top,
            };
        }
        return windowBounds;
    }

    private bool IsCloseWithoutSavingWindow(nint hwnd)
    {
        // Проверяем стили окна
        uint style = GetWindowStyle(hwnd);
        if ((style & WS_CAPTION) == 0 || (style & WS_SYSMENU) == 0)
            return true;

        // Проверяем текст окна

        return false;
    }
    const uint WS_OVERLAPPED = 0x00000000;


    const int GWL_STYLE = -16;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowLong(nint hWnd, int nIndex);
    private static bool IsOverlappedWindow(nint hwnd)
    {
        // Получаем стиль окна
        int style = GetWindowLong(hwnd, GWL_STYLE); // Используем GetWindowLong для получения стиля

        // Проверяем, имеет ли окно стиль WS_OVERLAPPED
        if ((style & (int)WS_OVERLAPPED) != 0)
            return true;

        // Если стиль не установлен, возвращаем false
        return false;
    }

    const uint WS_THICKFRAME = 0x00040000;
    private static bool IsThickFrameWindow(nint hwnd)
    {
        // Получаем стиль окна
        int style = GetWindowLong(hwnd, GWL_STYLE); // Используем GetWindowLong для получения стиля

        // Проверяем, имеет ли окно стиль WS_THICKFRAME
        if ((style & (int)WS_THICKFRAME) != 0)
            return true;

        // Если стиль не установлен, возвращаем false
        return false;
    }

    [DllImport("user32.dll")]
    private static extern uint GetWindowStyle(nint hWnd);

    private const uint WS_CAPTION = 0x00C00000;
    private const uint WS_SYSMENU = 0x00080000;

    [DllImport("user32.dll")]
    private static extern nint GetAncestor(nint hwnd, uint flags);

    private const uint GA_ROOT = 2;
}