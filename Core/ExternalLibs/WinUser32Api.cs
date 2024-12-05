using Core.Interfaces.ExternalLibs;
using System.Runtime.InteropServices;

namespace Core.ExternalLibs;

public class WinUser32Api : IWinUser32Api
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public static Rect Zero => new Rect();
    }

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    public IntPtr WaitForWindowOpen(int processId)
    {
        IntPtr foundWindow = IntPtr.Zero;

        EnumWindows((hWnd, lParam) =>
        {
            GetWindowThreadProcessId(hWnd, out uint windowProcessId);

            if (windowProcessId == processId && IsWindowVisible(hWnd))
            {
                foundWindow = hWnd;
                return false;
            }

            return true;
        }, IntPtr.Zero);

        return foundWindow;
    }

    public Rect GetWindowRect(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero)
        {
            return Rect.Zero;
        }
        GetWindowRect(hWnd, out Rect lpRect);
        return lpRect;
    }
}
