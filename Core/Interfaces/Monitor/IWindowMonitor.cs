namespace Core.Interfaces.Monitor;

public interface IWindowMonitor
{
    public bool Initialize();
    public void ProcessWindowInfoQueue();
}
