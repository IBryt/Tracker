namespace Core.Interfaces.Observer;

public interface IWindowObserverThread : IDisposable
{
    public void Start(CancellationToken token);
}
