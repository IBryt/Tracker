using Core.Enums;

namespace Core.Entities;

public class WindowAction
{
    public nint Hwnd { get; set; }
    public EventType EventType { get; set; }
    public WindowBounds? WindowBounds { get; set; }
    public override string ToString()
    {
        return $"WindowAction: Hwnd={Hwnd},EventType={EventType}, WindowBounds={(WindowBounds?.ToString() ?? "null")}";
    }
}
