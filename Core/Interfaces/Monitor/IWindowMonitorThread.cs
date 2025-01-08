namespace Core.Interfaces.Monitor;

public interface IWindowMonitorThread
{
    public void Start(CancellationToken token);
}
