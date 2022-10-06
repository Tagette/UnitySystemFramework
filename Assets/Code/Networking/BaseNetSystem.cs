using UnitySystemFramework.Core;
using System.Collections.Generic;
using UnitySystemFramework.Serialization;

namespace UnitySystemFramework.Networking
{
    /// <summary>
    /// A base class used to simplify the networking of a system. Automatically registers itself as a network handler.
    /// </summary>
    public abstract class BaseNetSystem : BaseSystem, ISystem, INetHandler
    {
        private TypeID _typeID;

        /// <summary>
        /// A cached reference to the network system.
        /// </summary>
        protected NetworkSystem NetworkSystem { get; private set; }

        /// <summary>
        /// Called when a buffer is received for this system.
        /// </summary>
        protected abstract void OnReceive(Network network, NetConnection connection, ByteBuffer buffer);

        void ISystem.OnInit()
        {
            _typeID = GetType().GetTypeID();
            NetworkSystem = Game.RequireSystem<NetworkSystem>();
            NetworkSystem.AddHandler(this);
            OnInit();
        }

        void INetHandler.OnReceive(Network network, NetConnection connection, ByteBuffer buffer)
        {
            OnReceive(network, connection, buffer);
        }

        void ISystem.OnEnd()
        {
            OnEnd();
            NetworkSystem.RemoveHandler(this);
            NetworkSystem = null;
        }

        /// <summary>
        /// Gets a buffer which can be used to write your data into and send over the network using <see cref="Send(ByteBuffer, NetConnection)"/>.
        /// </summary>
        protected ByteBuffer GetBuffer(int length = 1024) => NetworkSystem.GetBuffer(length);

        /// <summary>
        /// Releases the NetBuffer so that it's internal byte array can be pooled. A NetBuffer is automatically 
        /// released when used with Send(). To prevent this behaviour set <see cref="ByteBuffer.DoNotRelease"/> 
        /// to true.
        /// </summary>
        protected void ReleaseBuffer(ByteBuffer buffer) => NetworkSystem.ReleaseBuffer(buffer);

        /// <summary>
        /// Sends a buffer of data to the specified recipient. This system will automatically receive the data as
        /// long as the system also exists on the recipient. This also automatically releases the provided buffer 
        /// back into the pool. To prevent this behaviour set <see cref="ByteBuffer.DoNotRelease"/> to true.
        /// </summary>
        protected void Send(ByteBuffer buffer, NetConnection connection) => NetworkSystem.Send(_typeID, buffer, connection);

        /// <summary>
        /// Sends a buffer of data to the specified recipients. This system will automatically receive the data as
        /// long as the system also exists on the recipients. This also automatically releases the provided buffer 
        /// back into the pool. To prevent this behaviour set <see cref="ByteBuffer.DoNotRelease"/> to true.
        /// </summary>
        protected void Send(ByteBuffer buffer, Recipients recipients = null) => NetworkSystem.Send(_typeID, buffer, recipients);
    }
}
