using Core.Entities;
using Core.Native;

namespace Core.Interfaces.Native;

public interface IEventProcessor
{
    public void EventProcessorCallback(nint hWinEventHook, WinEvent eventType, nint hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
}
