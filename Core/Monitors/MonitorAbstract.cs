using Core.Interfaces.ExternalLibs;
using Core.Interfaces.Monitors;
using Microsoft.Extensions.Logging;

namespace Core.Monitors;

public class MonitorAbstract<T> : IBaseMonitor
{
    private readonly ILogger<T> _logger;
    private readonly IWinUser32Api _winUser32Api;

    public MonitorAbstract(ILogger<T> logger, IWinUser32Api winUser32Api)
    {
        _logger = logger;
        _winUser32Api = winUser32Api;
    }

    public void FindWindowByProcessId(int processId)
    {
        var hWnd = _winUser32Api.WaitForWindowOpen(processId);
        var rect = _winUser32Api.GetWindowRect(hWnd);
    }
}
