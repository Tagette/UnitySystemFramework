using UnitySystemFramework.Core;

namespace UnitySystemFramework.Networking
{
    public struct ClientStartEvent : IEvent
    {
        public Network Network;
    }

    public struct ClientConnectedEvent : IEvent
    {
        public Network Network;
        public NetConnection Connection;
    }

    public struct ClientShutdownEvent : IEvent
    {
        public Network Network;
    }

    public struct ServerStartEvent : IEvent
    {
        public Network Network;
    }

    public struct ServerAddClientEvent : IEvent
    {
        public Network Network;
        public NetConnection Connection;
    }

    public struct ServerRemoveClientEvent : IEvent
    {
        public bool WasKicked;
        public Network Network;
        public NetConnection Connection;
    }

    public struct ServerShutdownEvent : IEvent
    {
        public Network Network;
    }
}
