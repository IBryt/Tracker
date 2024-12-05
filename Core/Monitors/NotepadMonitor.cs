using Core.Interfaces.ExternalLibs;
using Core.Interfaces.Monitors;
using Core.Monitors;
using Microsoft.Extensions.Logging;

namespace Core;

public class NotepadMonitor : MonitorAbstract<INotepadMonitor>, INotepadMonitor
{
    public NotepadMonitor(ILogger<INotepadMonitor> logger, IWinUser32Api winUser32Api) : base(logger, winUser32Api)
    {
    }
}
