namespace Core.Native.Enums;

[Flags]
public enum WindowStylesEx : int
{
    WS_EX_NOACTIVATE = 0x08000000,
    WS_EX_TRANSPARENT = 0x00000020,
    WS_EX_LAYERED = 0x00080000,
}
