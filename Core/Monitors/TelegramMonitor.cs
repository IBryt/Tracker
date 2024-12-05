using Core.Interfaces.ExternalLibs;
using Core.Interfaces.Monitors;
using Microsoft.Extensions.Logging;

namespace Core.Monitors;

public class TelegramMonitor : MonitorAbstract<ITelegramMonitor>, ITelegramMonitor
{
    public TelegramMonitor(ILogger<ITelegramMonitor> logger, IWinUser32Api winUser32Api) : base(logger, winUser32Api)
    {
    }
}
