using System;

namespace UnitySystemFramework.Core
{
    [Flags]
    public enum EventOrders : byte
    {
        Earliest = 1 << 1,
        Early = 1 << 2,
        Normal = 1 << 3,
        Late = 1 << 4,
        Latest = 1 << 5,
    }
}
