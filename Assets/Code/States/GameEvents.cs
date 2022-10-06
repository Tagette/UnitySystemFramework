using UnitySystemFramework.Core;
using UnitySystemFramework.Networking;
using UnitySystemFramework.Serialization;

namespace UnitySystemFramework.States
{
    public struct SessionReadyEvent : INetworkEvent
    {
        public uint CompositeID { get; set; }

        public void OnRead(IGame game, ByteBuffer buffer)
        {
        }

        public void OnWrite(IGame game, ByteBuffer buffer)
        {
        }
    }

    public struct SessionOverEvent : INetworkEvent
    {
        public uint CompositeID { get; set; }

        public void OnRead(IGame game, ByteBuffer buffer)
        {
        }

        public void OnWrite(IGame game, ByteBuffer buffer)
        {
        }
    }

    public enum SessionCancelReason : byte
    {
        PlayerDisconnected,
        PlayerTimeout,
    }

    public struct SessionCancelEvent : INetworkEvent
    {
        public uint CompositeID { get; set; }

        public SessionCancelReason Reason;

        public void OnRead(IGame game, ByteBuffer buffer)
        {
            Reason = (SessionCancelReason) buffer.ReadByte();
        }

        public void OnWrite(IGame game, ByteBuffer buffer)
        {
            buffer.WriteByte((byte) Reason);
        }
    }
}
