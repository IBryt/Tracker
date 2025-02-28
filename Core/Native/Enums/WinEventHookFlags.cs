﻿namespace Core.Native.Enums;

[Flags]
public enum WinEventHookFlags : uint
{
    WINEVENT_OUTOFCONTEXT = 0x0000,
    WINEVENT_SKIPOWNTHREAD = 0x0001,
    WINEVENT_SKIPOWNPROCESS = 0x0002,
    WINEVENT_INCONTEXT = 0x0004
}