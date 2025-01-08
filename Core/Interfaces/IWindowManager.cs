using Core.Entities;

namespace Core.Interfaces;

public interface IWindowManager
{
    public void AddWindow(WindowInfo windowInfo);
    public void RemoveWindow(nint windowHandle);
}
