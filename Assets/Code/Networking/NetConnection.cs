using System;
using Unity.Networking.Transport;

namespace UnitySystemFramework.Networking
{
    public struct NetConnection : IEquatable<NetConnection>
    {
        /// <summary>
        /// Represents the local connection.
        /// </summary>
        public static NetConnection Local = new NetConnection()
        {
            Connection = new NetworkConnection(),
            NetworkID = int.MaxValue,
        };

        /// <summary>
        /// Whether or not this connection is still created and valid.
        /// </summary>
        public bool IsCreated => Connection.IsCreated;

        /// <summary>
        /// Whether or not this connection is the local representation.
        /// </summary>
        public bool IsLocal => NetworkID == int.MaxValue;

        /// <summary>
        /// The ID of the network this connection belongs to.
        /// </summary>
        public int NetworkID;

        /// <summary>
        /// The internal connection.
        /// </summary>
        public NetworkConnection Connection;

        public bool Equals(NetConnection other)
        {
            return NetworkID == other.NetworkID && Connection.Equals(other.Connection);
        }

        public override bool Equals(object obj)
        {
            return obj is NetConnection other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (NetworkID * 397) ^ Connection.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{Connection.InternalId}:{NetworkID}";
        }

        public static bool operator ==(NetConnection a, NetConnection b)
        {
            return a.NetworkID == b.NetworkID && a.Connection == b.Connection;
        }

        public static bool operator !=(NetConnection a, NetConnection b)
        {
            return !(a == b);
        }
    }
}
