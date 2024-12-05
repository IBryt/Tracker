using Core.Interfaces;
using Core.Interfaces.Monitors;
using Core.Utils;
using Microsoft.Extensions.Logging;

namespace Core;

public class Tracker: ITracker
{
    private readonly ILogger<ITracker> _logger;
    private readonly IAppMonitor _processTracker;
    private readonly INotepadMonitor _notepadMonitor;

    public Tracker(ILogger<ITracker> logger, IAppMonitor processTracker, INotepadMonitor notepadMonitor)
    {
        _logger = logger;
        _processTracker = processTracker;
        _notepadMonitor = notepadMonitor;
    }

    public async Task RunAsync(string[] args)
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;

        _processTracker.ProcessStarted += (sender, process) =>
        {
            _logger.LogDebug($"Process started: {process.ProcessName}, ID: {process.Id}");
            Task.Run(() => _notepadMonitor.FindWindowByProcessId(process.Id));
        };

        _processTracker.ProcessStoped += (sender, process) =>
        {
            _logger.LogDebug($"Process stoped: {process.ProcessName}, ID: {process.Id}");
            Task.Run(() => _notepadMonitor.FindWindowByProcessId(process.Id));
        };

        _logger.LogInformation("Application is starting...");
        await _processTracker.StartAsync(token);
        _logger.LogInformation("Application has stopped.");
    }

    //public void GetMonitorByProcessName(string name)
    //{ 
    //    if(name)
    //}

    private class TrackerHelper
    {
        private readonly INotepadMonitor _notepadMonitor;
        private readonly ITelegramMonitor _telegramMonitor;

        public TrackerHelper(INotepadMonitor notepadMonitor, ITelegramMonitor telegramMonitor)
        {
            _notepadMonitor = notepadMonitor;
            _telegramMonitor = telegramMonitor;
        }

        public IBaseMonitor? GetMonitorByProcessName(string name)
        {
            if (name == Constants.Notepad)
            {
                return _notepadMonitor;
            }

            if (name == Constants.Telegram)
            {
                return _telegramMonitor;
            }

            return null;
        }
    }
}

