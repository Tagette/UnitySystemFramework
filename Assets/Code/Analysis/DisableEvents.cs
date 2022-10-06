using UnitySystemFramework.Core;
using UnitySystemFramework.Networking;
using UnitySystemFramework.Serialization;

namespace UnitySystemFramework.Analysis
{
    public struct DisableEvent : INetworkEvent
    {
        public string Key;
        public bool IsEnabled;

        public uint CompositeID { get; set; }

        public void OnRead(IGame game, ByteBuffer buffer)
        {
            Key = buffer.ReadString();
            IsEnabled = buffer.ReadBool();
        }

        public void OnWrite(IGame game, ByteBuffer buffer)
        {
            buffer.WriteString(Key);
            buffer.WriteBool(IsEnabled);
        }
    }
}
