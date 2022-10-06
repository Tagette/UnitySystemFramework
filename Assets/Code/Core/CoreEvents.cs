namespace UnitySystemFramework.Core
{
    public struct SystemAddedEvent : IEvent
    {
        public ISystem System;
    }

    public struct SystemRemovedEvent : IEvent
    {
        public ISystem System;
    }

    public struct SystemEnableEvent : IEvent
    {
        public ISystem System;
    }

    public struct SystemDisableEvent : IEvent
    {
        public ISystem System;
    }

    public struct ScopeChangedEvent : IEvent
    {
        public ScopeKey Scope;
        public bool Enabled;
    }
}
