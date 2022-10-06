using UnitySystemFramework.Serialization;

namespace UnitySystemFramework.Networking
{
    public interface INetHandler
    {
        void OnReceive(Network network, NetConnection connection, ByteBuffer buffer);
    }
}
