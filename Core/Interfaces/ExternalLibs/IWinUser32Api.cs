using static Core.ExternalLibs.WinUser32Api;

namespace Core.Interfaces.ExternalLibs;

public interface IWinUser32Api
{
    public IntPtr WaitForWindowOpen(int processId);
    public Rect GetWindowRect(IntPtr hWnd);
}
