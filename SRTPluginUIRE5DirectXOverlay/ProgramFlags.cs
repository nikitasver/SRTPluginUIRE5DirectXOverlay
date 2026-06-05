using System;

namespace SRTPluginUIRE5DirectXOverlay
{
    [Flags]
    public enum ProgramFlags : byte
    {
        None = 0,
        Debug = 1,
        NoTitleBar = 4,
        AlwaysOnTop = 8,
        Transparent = 16,
        NoInventory = 32,
    }
}
