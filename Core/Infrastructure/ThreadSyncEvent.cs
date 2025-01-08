using Core.Common;
using Core.Interfaces.Infrastructure;

namespace Core.Infrastructure;

public class ThreadSyncEvent : IThreadSyncEvent
{
    private AutoResetEvent? _event;

    public void Initialization()
    {
        _event = new AutoResetEvent(false);
    }

    public void WaitWithTimeout()
    {
        _event?.WaitOne(Constants.AutoResetEventTimeout);
    }

    public void Wait()
    {
        _event?.WaitOne();
    }

    public void Reset()
    {
        _event?.Set();
    }

    public void Dispose()
    {
        _event?.Dispose();
    }
}
