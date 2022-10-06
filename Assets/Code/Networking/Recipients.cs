using System;
using System.Collections;
using System.Collections.Generic;
using UnitySystemFramework.Collections;
using UnitySystemFramework.Core;

namespace UnitySystemFramework.Networking
{
    public class Recipients : IList<NetConnection>, IDisposable
    {
        private static readonly CapacityPool<List<NetConnection>> _pool = new CapacityPool<List<NetConnection>>(
            capacity => new List<NetConnection>(capacity),
            recipients => recipients.Count,
            list => list.Clear());

        public static Recipients GetRecipients(int length)
        {
            if (length == 0)
                return new Recipients();
            return new Recipients(_pool.Get(length));
        }

        public static void ReleaseRecipients(Recipients recipients)
        {
            if (recipients._doNotRelease)
                return;

            _pool.Release(recipients._recipients);
            recipients._recipients = null;
        }

        /// <summary>
        /// Returns an empty recipient list with 0 capacity.
        /// </summary>
        public static Recipients Empty => GetRecipients(0);

        /// <summary>
        /// Gets the local connection only in a recipient list.
        /// </summary>
        public static Recipients Local => From(NetConnection.Local);

        /// <summary>
        /// Gets all connections in a recipient list.
        /// </summary>
        public static Recipients All
        {
            get
            {
                var game = Game.CurrentGame;
                var networkSystem = game.GetSystem<NetworkSystem>();
                return From(networkSystem.Connections);
            }
        }

        /// <summary>
        /// All connections across all initialized networks.
        /// </summary>
        public static Recipients OtherConnections
        {
            get
            {
                var game = Game.CurrentGame;
                var networkSystem = game.GetSystem<NetworkSystem>();
                var all = networkSystem.Connections;
                var recipients = GetRecipients(all.Count);
                recipients._recipients.AddRange(all);
                if (networkSystem.ServerConnection.IsCreated)
                    recipients.Add(networkSystem.ServerConnection);
                return recipients;
            }
        }

        /// <summary>
        /// Returns a recipient list with just the server connection in it if there is one.
        /// </summary>
        public static Recipients Server
        {
            get
            {
                var game = Game.CurrentGame;
                var networkSystem = game.GetSystem<NetworkSystem>();
                var recipients = GetRecipients(1);
                if (!networkSystem.IsListening && networkSystem.ServerConnection.IsCreated)
                    recipients.Add(networkSystem.ServerConnection);

                if(networkSystem.IsListening)
                    recipients.Add(NetConnection.Local);

                return recipients;
            }
        }

        public static Recipients OtherConnectionsExcept(NetConnection connection)
        {
            var otherConnections = OtherConnections;
            otherConnections.Remove(connection);
            return otherConnections;
        }

        public static Recipients OtherConnectionsExcept(NetConnection connection1, NetConnection connection2)
        {
            var otherConnections = OtherConnections;
            otherConnections.Remove(connection1);
            otherConnections.Remove(connection2);
            return otherConnections;
        }

        public static Recipients OtherConnectionsExcept(NetConnection connection1, NetConnection connection2, NetConnection connection3)
        {
            var otherConnections = OtherConnections;
            otherConnections.Remove(connection1);
            otherConnections.Remove(connection2);
            otherConnections.Remove(connection3);
            return otherConnections;
        }

        public static Recipients OtherConnectionsExcept(params NetConnection[] connections)
        {
            var otherConnections = OtherConnections;
            foreach (var connection in connections)
                otherConnections.Remove(connection);
            return otherConnections;
        }

        public static Recipients From(NetConnection connection)
        {
            if (!connection.IsCreated && !connection.IsLocal)
                return Empty;
            var recipients = GetRecipients(1);
            recipients.Add(connection);
            return recipients;
        }

        public static Recipients From(NetConnection connection1, NetConnection connection2)
        {
            var recipients = GetRecipients(2);
            recipients.Add(connection1);
            recipients.Add(connection2);
            return recipients;
        }

        public static Recipients From(NetConnection connection1, NetConnection connection2, NetConnection connection3)
        {
            var recipients = GetRecipients(3);
            recipients.Add(connection1);
            recipients.Add(connection2);
            recipients.Add(connection3);
            return recipients;
        }

        public static Recipients From(params NetConnection[] connections)
        {
            var recipients = GetRecipients(connections.Length);
            foreach (var connection in connections)
                recipients.Add(connection);
            return recipients;
        }

        public static Recipients From(ICollection<NetConnection> connections)
        {
            var recipients = GetRecipients(connections.Count);
            foreach (var connection in connections)
                recipients.Add(connection);

            return recipients;
        }

        public static Recipients From(IEnumerable<NetConnection> connections)
        {
            var recipients = GetRecipients(10);
            foreach (var connection in connections)
                recipients.Add(connection);

            return recipients;
        }

        public static implicit operator Recipients(NetConnection connection)
        {
            return From(connection);
        }

        private bool _doNotRelease;
        private List<NetConnection> _recipients;

        private Recipients()
        {
        }

        private Recipients(List<NetConnection> recipients)
        {
            _recipients = recipients;
        }

        public int Count => _recipients?.Count ?? 0;

        public void SetDoNotDispose(bool doNotDispose)
        {
            _doNotRelease = !doNotDispose;
        }

        public void Dispose()
        {
            ReleaseRecipients(this);
        }

        #region IList<>

        public NetConnection this[int index]
        {
            get => _recipients[index];
            set => _recipients[index] = value;
        }

        public int IndexOf(NetConnection item)
        {
            return _recipients?.IndexOf(item) ?? -1;
        }

        public void Insert(int index, NetConnection item)
        {
            if (_recipients == null && index == 0)
                _recipients = _pool.Get(1);
            _recipients?.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _recipients?.RemoveAt(index);
        }

        #endregion // IList<>

        #region ICollection<>

        bool ICollection<NetConnection>.IsReadOnly => false;

        public bool Contains(NetConnection item)
        {
            return _recipients?.Contains(item) ?? false;
        }

        public void Add(NetConnection item)
        {
            if (_recipients == null)
                _recipients = _pool.Get(1);
            _recipients.Add(item);
        }

        public bool Remove(NetConnection item)
        {
            return _recipients?.Remove(item) ?? false;
        }

        public void Clear()
        {
            _recipients?.Clear();
        }

        public void CopyTo(NetConnection[] array, int arrayIndex)
        {
            _recipients?.CopyTo(array, arrayIndex);
        }

        #endregion // ICollection<>

        #region IEnumerator<>

        public IEnumerator<NetConnection> GetEnumerator()
        {
            return _recipients?.GetEnumerator() ?? new List<NetConnection>(0).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion // IEnumerator<>
    }

    public static class RecipientExtensions
    {
        public static Recipients ToRecipients(this IEnumerable<NetConnection> connections)
        {
            return Recipients.From(connections);
        }
    }
}
