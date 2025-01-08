using Core.Entities;
using Core.Interfaces;
using Core.Native.Enums;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Core.Native;

public class WindowManager : IWindowManager
{
    private readonly Dictionary<nint, nint> _windows = new();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint CreateWindowEx(
    uint dwExStyle, string lpClassName, string lpWindowName,
    uint dwStyle, int x, int y, int nWidth, int nHeight,
    nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(nint hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    static extern bool UpdateWindow(nint hWnd);

    [DllImport("user32.dll")]
    static extern bool GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll")]
    static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll")]
    static extern nint DispatchMessage([In] ref MSG lpMsg);

    //[DllImport("user32.dll")]
    //static extern bool RegisterClassEx(ref WNDCLASSEX lpwcx);

    [DllImport("user32.dll")]
    public static extern nint DefWindowProc(nint hWnd, uint msg, nint wParam, nint lParam);

    [DllImport("user32.dll")]
    static extern bool GetWindowRect(nint hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    static extern ushort GetClassWord(nint hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool UnregisterClass(string lpClassName, nint hInstance);


    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public nint hwnd;
        public uint message;
        public nint wParam;
        public nint lParam;
        public uint time;
        public POINT pt;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyWindow(nint hWnd);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct WNDCLASSEX
    {
        public uint cbSize;
        public WindowClassStyles style;
        public WndProcDelegate lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public nint hInstance;
        public nint hIcon;
        public nint hCursor;
        public nint hbrBackground;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpszMenuName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpszClassName;
        public nint hIconSm;
    }

    public delegate nint WndProcDelegate(nint hWnd, uint msg, nint wParam, nint lParam);

    // Constants
    private const int SW_SHOW = 5;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_POPUP = 0x80000000;
    private const uint WS_BORDER = 0x00800000;
    private const uint WS_THICKFRAME = 0x00040000;
    private const uint WS_EX_TOPMOST = 0x00000008;
    private const uint WS_EX_TOOLWINDOW = 0x00000080;
    private const uint CS_VREDRAW = 0x0001;
    private const uint CS_HREDRAW = 0x0002;
    private const uint CS_GLOBALCLASS = 0x4000;
    private const uint COLOR_WINDOW = 5;
    private const uint WM_DESTROY = 0x0002;
    private const uint WM_NCDESTROY = 0x0082;
    private const uint WM_PAINT = 0x000F;
    private const uint WM_ACTIVATE = 0x0006;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const int HWND_TOPMOST = -1;

    private static string CLASS_NAME = "TopMostChildWindow";
    private static ConcurrentDictionary<nint, Thread> windowThreads = new ConcurrentDictionary<nint, Thread>();
    private static WndProcDelegate wndProcDelegate = new WndProcDelegate(WndProc);
    private static bool isClassRegistered = false;

    public void AddWindow(WindowInfo windowInfo)
    {
        if (!isClassRegistered)
        {
            //if (!RegisterWindowClass())
            //{
            //    Console.WriteLine($"Ошибка регистрации класса окна: {Marshal.GetLastWin32Error()}");
            //    return;
            //}
            isClassRegistered = true;
        }

        CreateWindowThread(windowInfo.ParentHwnd);
    }



    static void CreateWindowThread(nint parentHandle)
    {
        try
        {
            Console.WriteLine($"Создание окна для родительского хэндла: {parentHandle}");

            // Получаем размеры родительского окна
            RECT parentRect;
            GetWindowRect(parentHandle, out parentRect);

            // Создаем окно поверх родительского
            nint childWindow = CreateWindowEx(
                WS_EX_TOPMOST | WS_EX_TOOLWINDOW,  // dwExStyle
                CLASS_NAME,       // className
                "Top Window",     // windowName
                WS_POPUP | WS_VISIBLE | WS_BORDER | WS_THICKFRAME,  // dwStyle
                parentRect.Left + 10,     // x
                parentRect.Top + 10,      // y
                200,                      // width
                100,                      // height
                parentHandle,             // parent window
                nint.Zero,              // menu
                Process.GetCurrentProcess().Handle,  // instance
                nint.Zero               // param
            );

            if (childWindow == nint.Zero)
            {
                Console.WriteLine($"Ошибка создания окна: {Marshal.GetLastWin32Error()}");
                return;
            }

            Console.WriteLine($"Окно создано успешно, handle: {childWindow}");

            // Устанавливаем окно поверх всех окон
            SetWindowPos(childWindow, new nint(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);

            ShowWindow(childWindow, SW_SHOW);
            UpdateWindow(childWindow);

            Console.WriteLine("Запуск цикла сообщений");
            //RunMessageLoop();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в потоке окна: {ex.Message}");
        }
    }

    //static bool RegisterWindowClass()
    //{
    //    var wndClass = new WNDCLASSEX
    //    {
    //        cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX)),
    //        style = CS_VREDRAW | CS_HREDRAW | CS_GLOBALCLASS,  // Добавлен CS_GLOBALCLASS
    //        lpfnWndProc = wndProcDelegate,
    //        cbClsExtra = 0,
    //        cbWndExtra = 0,
    //        hInstance = Process.GetCurrentProcess().Handle,
    //        hIcon = nint.Zero,
    //        hCursor = nint.Zero,
    //        hbrBackground = (nint)(COLOR_WINDOW + 1),
    //        lpszMenuName = null,
    //        lpszClassName = CLASS_NAME,
    //        hIconSm = nint.Zero
    //    };

    //    return RegisterClassEx(ref wndClass);
    //}

    static nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        switch (msg)
        {
            case WM_ACTIVATE:
                SetWindowPos(hWnd, new nint(HWND_TOPMOST), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                break;
            case WM_DESTROY:
                return nint.Zero;
        }
        return DefWindowProc(hWnd, msg, wParam, lParam);
    }

    public static void RunMessageLoop()
    {
        MSG msg;
        while (GetMessage(out msg, nint.Zero, 0, 0))
        {
            TranslateMessage(ref msg);
            DispatchMessage(ref msg);

        }
    }


    public void RemoveWindow(nint windowHandle)
    {
        if (windowHandle != nint.Zero)
        {
            _windows.Remove(windowHandle, out nint windowHwnd);
            if (windowHwnd != nint.Zero)
            {
                DestroyWindow(windowHwnd);
            }
        }
    }
}
