using Core.Enums;
using Core.Native.Enums;

namespace Core.Entities;

public class WindowInfo
{
    public nint ParentHwnd { get; set; }
    public WindowEvent WindowEvent { get; set; }
    public WindowBounds? WindowBounds { get; set; }
    public SystemProcesses? SystemProcess { get; set; }
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;

    public override string ToString()
    {
        return $"windowInfo: ParentHwnd={ParentHwnd}, SystemProcess={SystemProcess}, WindowEvent={WindowEvent}, WindowBounds={(WindowBounds?.ToString() ?? "null")}, CreatedTime={CreatedTime}.";
    }
}
