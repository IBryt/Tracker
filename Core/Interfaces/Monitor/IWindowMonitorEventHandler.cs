using Core.Entities;

namespace Core.Interfaces.Monitor;

public interface IWindowMonitorEventHandler
{
    public void HandleWindowEvent(WindowInfo? windowInfo);
}
