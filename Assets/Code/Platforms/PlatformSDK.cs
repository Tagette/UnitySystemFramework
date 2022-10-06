using System;

namespace UnitySystemFramework.Platforms
{
    [Flags]
    public enum PlatformSDK
    {
        None = 0,
        Custom = (1 << 0),
        Steam = (1 << 1),
        Epic = (1 << 2),
        GOG = (1 << 3),
        BattleNet = (1 << 4),
        Origin = (1 << 5),
        UbisoftConnect = (1 << 6),
        GamePass = (1 << 7),
        Apple = (1 << 8),
        Google = (1 << 9),
        Xbox360 = (1 << 10),
        XboxOne = (1 << 11),
        XboxGameCore = (1 << 12),
        PS3 = (1 << 13),
        PS4 = (1 << 14),
        PS5 = (1 << 15),
        Stadia = (1 << 16),
        Switch = (1 << 17),
    }
}
