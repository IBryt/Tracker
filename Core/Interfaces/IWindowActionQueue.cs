using Core.Entities;

namespace Core.Interfaces;

public interface IWindowActionQueue
{
    public void Add(WindowAction action);
    public WindowAction Take();
    public bool IsEmpty();
}