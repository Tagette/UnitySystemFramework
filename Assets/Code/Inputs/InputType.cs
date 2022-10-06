using System;

namespace UnitySystemFramework.Inputs
{
    [Flags]
    public enum InputType : byte
    {
        None = 0,
        Press = 1,
        Down = 1 << 1,
        Hold = 1 << 2,
        Up = 1 << 3,
        DoubleDown = 1 << 4,
        All = Press | Down | Hold | Up | DoubleDown,
    }
}
