namespace Core.Native.Enums;

[Flags]
public enum WindowClassStyles : uint
{
    CS_VREDRAW = 0x0001,
    CS_HREDRAW = 0x0002,
    CS_GLOBALCLASS = 0x4000
}
