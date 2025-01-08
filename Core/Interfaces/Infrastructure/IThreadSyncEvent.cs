namespace Core.Interfaces.Infrastructure;

public interface IThreadSyncEvent : IDisposable
{
    public void Initialization();
    public void Wait();
    public void WaitWithTimeout();
    public void Reset();
}
