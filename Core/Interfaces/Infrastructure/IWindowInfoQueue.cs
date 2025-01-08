using Core.Entities;

namespace Core.Interfaces.Infrastructure;

public interface IWindowInfoQueue
{
    public void Add(WindowInfo windowInfo);
    public bool TryDequeue(out WindowInfo? windowInfo);
}