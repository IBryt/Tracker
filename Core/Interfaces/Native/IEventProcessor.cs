using Core.Entities;
using Core.Native.Enums;

namespace Core.Interfaces.Native;

public interface IEventProcessor
{
    public void EventProcessorCallback(nint hWinEventHook, WindowEvent eventType, nint hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
}
