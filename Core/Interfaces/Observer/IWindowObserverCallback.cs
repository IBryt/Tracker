using Core.Native.Enums;

namespace Core.Interfaces.Observer;

public interface IWindowObserverCallback
{
    public void HandleWindowEvent(nint hWinEventHook, WindowEvent eventType, nint hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
}
