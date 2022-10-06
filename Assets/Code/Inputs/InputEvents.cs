using UnitySystemFramework.Core;
using UnityEngine;

namespace UnitySystemFramework.Inputs
{
    public struct MouseCollideEvent : IEvent
    {
        public MouseEventType Type;
        public MouseButton Button;
        public Collider Collider;
    }

    public struct InputEvent : IEvent
    {
        public InputKey Key;
        public InputType Type;
    }

    public struct InputEnableEvent : IEvent
    {
        public InputKey Key;
        public bool Enable;
    }
}
