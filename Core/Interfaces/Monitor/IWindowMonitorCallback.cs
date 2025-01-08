using Core.Native.Enums;

namespace Core.Interfaces.Monitor;

public interface IWindowMonitorCallback
{
    public nint OnWindowMessage(nint hWnd, WindowMessages msg, nint wParam, nint lParam);
}
