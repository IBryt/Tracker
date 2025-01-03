using Core.Entities;

namespace Core.Interfaces.Native;

public interface IWindowManager
{
    public void AddWindow(WindowAction windowAction);
    public void RemoveWindow(IntPtr windowHandle);
}
