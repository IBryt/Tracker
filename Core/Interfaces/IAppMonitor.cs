using static Core.AppMonitor;

namespace Core.Interfaces;

public interface IAppMonitor
{
    public event ProcessStartedEventHandler? ProcessStarted;
    public event ProcessStopedEventHandler? ProcessStoped;

    public Task StartAsync(CancellationToken cancellationToken);
}
