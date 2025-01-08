namespace Core.Native.Enums;

[Flags]
public enum WindowDwStyle : uint
{
    WS_VISIBLE = 0x10000000,
    WS_POPUP = 0x80000000,
    WS_BORDER = 0x00800000,
    WS_THICKFRAME = 0x00040000,
}
