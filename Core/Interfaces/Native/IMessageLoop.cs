namespace Core.Interfaces.Native;

public interface IMessageLoop : IDisposable
{
    public void Start();
}
