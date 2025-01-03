using System.Runtime.InteropServices;
using static Core.Native.EventProcessor;

namespace Core.Native;

public static class NativeMethods
{

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetMessage(out MSG lpMsg, nint hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool TranslateMessage([In] ref MSG lpMsg);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern nint DispatchMessage([In] ref MSG lpmsg);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern uint GetWindowThreadProcessId(nint hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern nint SetWinEventHook(WinEvent eventMin, WinEvent eventMax, nint hmodWinEventProc, WindowEventProcDelegate lpfnWinEventProc, uint idProcess, uint idThread, WinEventHookFlags dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool UnhookWinEvent(nint hWinEventHook);

    [StructLayout(LayoutKind.Sequential)]
    public struct MSG
    {
        public nint hwnd;
        public uint message;
        public nuint wParam;
        public nint lParam;
        public int time;
        public POINT pt;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator System.Drawing.Point(POINT p) => new System.Drawing.Point(p.X, p.Y);

        public static implicit operator POINT(System.Drawing.Point p) => new POINT(p.X, p.Y);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetWindowRect(nint hwnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
