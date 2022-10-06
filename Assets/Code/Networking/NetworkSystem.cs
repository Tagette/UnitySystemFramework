using JetBrains.Annotations;
using UnitySystemFramework.Collections;
using UnitySystemFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Unity.Networking.Transport;
using UnityEngine;
using UnitySystemFramework.Serialization;

namespace UnitySystemFramework.Networking
{
    public class NetworkSystem : BaseSystem, INetHandler
    {
        public const int MTU_SIZE = 1300;
        public const ushort NET_VERSION = 1;

        public enum PacketType : byte
        {
            Ping,
            Pong,
            Event,
        }

        private readonly List<Network> _networks = new List<Network>();
        private readonly List<NetConnection> _connections = new List<NetConnection>();
        private readonly Dictionary<int, Network> _networkLookup = new Dictionary<int, Network>();
        private readonly Dictionary<TypeID, INetHandler> _handlers = new Dictionary<TypeID, INetHandler>();
        private readonly ByteBuffer _recvBuffer = new ByteBuffer(new byte[16384]);
        private readonly CapacityPool<byte[]> _bufferPool = new CapacityPool<byte[]>(capacity => new byte[capacity], buffer => buffer.Length);
        private readonly Dictionary<NetConnection, float> _lastPing = new Dictionary<NetConnection, float>();
        private readonly Dictionary<NetConnection, float> _roundTripTimes = new Dictionary<NetConnection, float>();
        private readonly Dictionary<int, Action<Network, NetConnection>> _tempConnects = new Dictionary<int, Action<Network, NetConnection>>();

        private readonly HashSet<int> _shuttingDown = new HashSet<int>();
        private uint _receivingEventID;

        private readonly MethodInfo _genericReadEvent;
        private readonly Dictionary<TypeID, MethodInfo> _eventReadMethods = new Dictionary<TypeID, MethodInfo>();

        public bool IsReceivingAnEvent => _receivingEventID != 0;

        public NetworkSystem()
        {
            _recvBuffer.DoNotRelease = true;
            _bufferPool.SetMinCapacity(1024);
            _bufferPool.SetMax(1024, 1000);
            _bufferPool.SetMax(1024 * 4, 200);
            _bufferPool.SetMax(1024 * 16, 50);
            _bufferPool.SetMax(1024 * 64, 10);
            _bufferPool.SetMax(1024 * 256, 3);
            _bufferPool.SetMax(1024 * 1024, 1);
            _bufferPool.SetDefaultMax(0);
            _genericReadEvent = GetType().GetMethod(nameof(ReadAndCallEvent), BindingFlags.NonPublic | BindingFlags.Instance);

            Networks = _networks.AsReadOnly();
            Connections = _connections.AsReadOnly();
        }

        /// <summary>
        /// The list of all running networks.
        /// </summary>
        public IReadOnlyList<Network> Networks { get; }

        /// <summary>
        /// Gets a list of all connections.
        /// </summary>
        public IReadOnlyList<NetConnection> Connections { get; }

        /// <summary>
        /// The server's connection. Should only be used as a client when there is only 1 network connection.
        /// </summary>
        public NetConnection ServerConnection { get; private set; }

        /// <summary>
        /// Determines if any network is currently listening.
        /// </summary>
        public bool IsListening => Networks.Any(n => n.IsListening);

        protected override void OnInit()
        {
        }

        protected override void OnStart()
        {
        }

        //private void OnNetworkEventEarliest(ref INetworkEvent e)
        //{
        //    if (GetGlobal("IsServer"))
        //    {
        //        if (e.GetRecipients() == null)
        //            e.SetRecipients(Recipients.OtherPlayersAndLocal);

        //        if (!e.GetRecipients().Contains(NetConnection.Local))
        //        {
        //            CancelCurrentEvent();
        //            SendEvent(e);
        //        }
        //    }
        //    else
        //    {
        //        if (!IsReceivingAnEvent || _receivingEventID != e.CompositeID)
        //        {
        //            // Clients can only send to server.
        //            e.SetRecipients(Recipients.Server);

        //            if (e.GetNetworkMode() == NetworkMode.ClientPrediction)
        //                e.GetRecipients().Add(NetConnection.Local);

        //            if (!e.GetRecipients().Contains(NetConnection.Local))
        //                CancelCurrentEvent();

        //            SendEvent(e);
        //        }
        //    }
        //}

        //private void OnNetworkEventLatest(ref INetworkEvent e)
        //{
        //    if (!GetGlobal("IsServer"))
        //        return;

        //    SendEvent(e);
        //}

        private void OnUpdate()
        {
            for (int i = 0; i < _networks.Count; i++)
            {
                var network = _networks[i];

                network.Update();

                if (!network.IsCreated)
                {
                    LogError("A network was disposed before it could be removed from the network list.");
                    Shutdown(network);
                    i--;

                    continue;
                }

                if (network.IsListening)
                {
                    while (network.Accept(out var newConnection))
                    {
                        //Log($"Accepting new client. ({newConnection.Connection.InternalId})");
                        _lastPing[newConnection] = Time.time;
                        _connections.Add(newConnection);

                        CallEvent(new ServerAddClientEvent()
                        {
                            Network = network,
                            Connection = newConnection,
                        });
                    }
                }

                NetworkEvent.Type type;
                while ((type = network.PopEvent(out var connection, out var reader)) != NetworkEvent.Type.Empty)
                {
                    if (type == NetworkEvent.Type.Connect)
                    {
                        if (_tempConnects.TryGetValue(network.ID, out var onConnect))
                        {
                            onConnect?.Invoke(network, connection);
                            _tempConnects.Remove(network.ID);
                            Shutdown(network);
                            break;
                        }

                        ServerConnection = connection;
                        _connections.Add(connection);

                        // Connect is not called on the server.
                        CallEvent(new ClientConnectedEvent()
                        {
                            Network = network,
                            Connection = connection,
                        });
                    }
                    else if (type == NetworkEvent.Type.Disconnect)
                    {
                        //Log($"Client has disconnected. ({connection.Connection.InternalId})");

                        if (network.IsListening)
                        {
                            Disconnect(connection);
                            CallEvent(new ServerRemoveClientEvent()
                            {
                                Network = network,
                                Connection = connection,
                                WasKicked = true,
                            });
                        }
                        else
                            Shutdown(network); // This will dispose the network so we can clean it up after we break.

                        break;
                    }
                    else if (type == NetworkEvent.Type.Data)
                    {
                        // TODO: Make it so only the first packet sends the version.
                        int version = reader.ReadUShort();

                        if(version != NET_VERSION)
                            continue;

                        int currentPacket = reader.ReadByte();
                        int totalPackets = reader.ReadByte();
                        var typeID = new TypeID(reader.ReadInt());
                        //Log($"Packet received for {typeID.Type.Name}. ({connection.Connection.InternalId}, {reader.Length - reader.GetBytesRead()})");

                        if (_handlers.TryGetValue(typeID, out var handler))
                        {
                            if (currentPacket == 1)
                                _recvBuffer.Reset();

                            int length = reader.Length - reader.GetBytesRead();
                            unsafe
                            {
                                fixed (byte* buffer = _recvBuffer._Data)
                                {
                                    reader.ReadBytes(buffer + _recvBuffer._Length, length);
                                }
                            }
                            _recvBuffer._Length += length;

                            if (currentPacket == totalPackets)
                                handler.OnReceive(network, connection, _recvBuffer);
                        }

                        // The only way this could happen is if we shut down the network.
                        if (!network.IsCreated)
                        {
                            // The network has already been removed so the code below this loop won't run. Lets decrement ourselves.
                            i--;
                            break;
                        }
                    }

                    if (!network.IsCreated)
                        break;
                }

                if (!network.IsCreated)
                {
                    if (RemoveNetwork(network))
                    {
                        Shutdown(network);
                        i--;
                    }

                    continue;
                }

                foreach (var connection in network.Connections)
                {
                    if (_lastPing.TryGetValue(connection, out float lastPing) && Time.time - lastPing > 1f)
                    {
                        _lastPing[connection] = Time.time;
                        var send = GetBuffer();
                        send.WriteByte((byte)PacketType.Ping);
                        Send<NetworkSystem>(send, connection);
                    }
                }
            }
        }

        void INetHandler.OnReceive(Network network, NetConnection connection, ByteBuffer buffer)
        {
            var packetType = (PacketType) buffer.ReadByte();

            switch (packetType)
            {
                case PacketType.Ping:
                {
                    var send = GetBuffer();
                    send.WriteByte((byte) PacketType.Pong);
                    Send<NetworkSystem>(send, connection);
                    break;
                }
                case PacketType.Pong:
                {
                    if (_lastPing.TryGetValue(connection, out float lastPing))
                        _roundTripTimes[connection] = Time.time - lastPing;
                    break;
                }
                case PacketType.Event:
                {
                    var typeID = new TypeID(buffer.ReadInt());
                    var type = typeID.Type;

                    if (!type.IsValueType)
                    {
                        LogError($"A non-struct event type was received. ({type.Name})");
                        return;
                    }

                    if (!_eventReadMethods.TryGetValue(typeID, out var eventReadMethod))
                        _eventReadMethods[typeID] = eventReadMethod = _genericReadEvent.MakeGenericMethod(type);

                    eventReadMethod.Invoke(this, new object[] {buffer, connection});
                    break;
                }
            }
        }

        [UsedImplicitly]
        private void ReadAndCallEvent<T>(ByteBuffer recvBuffer, NetConnection connection) where T : struct, INetworkEvent
        {
            T e = default;
            var netComponent = new NetworkEventComponent()
            {
                Sender = connection,
                Mode = NetworkMode.FullAuthoritative,
                Recipients = Recipients.All,
            };
            e.SetComponent(netComponent);

            var eventBuffer = GetBuffer();
            recvBuffer.ReadBuffer(eventBuffer);

            _receivingEventID = e.CompositeID;

            try
            {
                e.OnRead(Game, eventBuffer);
            }
            catch (Exception ex)
            {
                LogException(ex);
                return;
            }
            ReleaseBuffer(eventBuffer);

            int componentCount = recvBuffer.ReadInt();
            var componentsBuffer = GetBuffer();
            recvBuffer.ReadBuffer(componentsBuffer);
            for (int i = 0; i < componentCount; i++)
            {
                var componentType = new TypeID(componentsBuffer.ReadInt());
                var component = (INetworkEventComponent) FormatterServices.GetUninitializedObject(componentType);
                var componentBuffer = GetBuffer();
                componentsBuffer.ReadBuffer(componentBuffer);
                try
                {
                    component.OnRead(Game, componentBuffer);

                    e.SetComponent(component);
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
                ReleaseBuffer(componentBuffer);
            }
            ReleaseBuffer(componentsBuffer);

            CallEvent(e);

            _receivingEventID = 0;
        }

        // TODO: Make this not box and have same functionality.
        private void SendEvent(INetworkEvent e)
        {
            var typeID = e.GetType().GetTypeID();
            var recipients = e.GetRecipients();
            if (recipients == null || recipients.Count == 0)
            {
                LogWarning($"An {typeID.Name} was sent to 0 recipients. Set the recipients to Local to rid of this message.");
                return;
            }

            var sendBuffer = GetBuffer();

            sendBuffer.WriteByte((byte)PacketType.Event);
            sendBuffer.WriteInt(typeID._ID);

            var eventBuffer = GetBuffer();

            bool error = false;
            try
            {
                e.OnWrite(Game, eventBuffer);
            }
            catch (Exception ex)
            {
                error = true;
                LogException(ex);
            }

            if (!error)
                sendBuffer.WriteBuffer(eventBuffer);

            if (!error)
            {
                var components = e.GetComponents();
                int componentCount = 0;
                var componentsBuffer = GetBuffer();

                for (int i = 0; i < components.Count; i++)
                {
                    var component = components[i];
                    if (component is INetworkEventComponent netComponent)
                    {
                        var componentBuffer = GetBuffer();
                        try
                        {
                            netComponent.OnWrite(Game, componentBuffer);
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                            ReleaseBuffer(componentBuffer);
                            continue;
                        }

                        componentCount++;
                        componentsBuffer.WriteInt(component.GetType().GetTypeID()._ID);
                        componentsBuffer.WriteBuffer(componentBuffer);
                        ReleaseBuffer(componentBuffer);
                    }
                }

                sendBuffer.WriteInt(componentCount);
                sendBuffer.WriteBuffer(componentsBuffer);
                ReleaseBuffer(componentsBuffer);
            }

            if (!error)
                Send<NetworkSystem>(sendBuffer, e.GetRecipients());
            else
                InnerReleaseBuffer(sendBuffer, true);
        }

        protected override void OnEnd()
        {
            Shutdown();
        }

        /// <summary>
        /// Gets a buffer which can be used to write your data into and send over the network using <see cref="Send(ByteBuffer, NetConnection)"/>.
        /// </summary>
        public ByteBuffer GetBuffer(int length = 1024)
        {
            return new ByteBuffer(_bufferPool.Get(length));
        }

        /// <summary>
        /// Releases the NetBuffer so that it's internal byte array can be pooled. A NetBuffer is automatically 
        /// released when used with Send(). To prevent this behaviour set <see cref="ByteBuffer.DoNotRelease"/> 
        /// to true. Calling this method will ignore the DoNotRelease flag.
        /// </summary>
        public void ReleaseBuffer(ByteBuffer buffer)
        {
            InnerReleaseBuffer(buffer, true);
        }

        private void InnerReleaseBuffer(ByteBuffer buffer, bool ignore)
        {
            if (!ignore && buffer.DoNotRelease)
                return;

            _bufferPool.Release(buffer._Data);
            buffer._Data = null;
        }

        public Network Connect(string host, ushort port)
        {
            //Log($"Connecting to {host}:{port}.");
            var network = Network.Connect(host, port);
            if (network != null)
            {
                _networks.Add(network);
                _networkLookup.Add(network.ID, network);
                if (_networks.Count == 1)
                {
                    AddHandler(this);
                    //SubscribeInterface<INetworkEvent>(OnNetworkEventEarliest, EventOrders.Earliest);
                    //if (GetGlobal("IsServer"))
                    //    SubscribeInterface<INetworkEvent>(OnNetworkEventLatest, EventOrders.Latest);
                    AddUpdate(OnUpdate);
                }

                CallEvent(new ClientStartEvent() {Network = network});
            }

            return network;
        }

        public Network Listen(string bindIP, ushort port)
        {
            Log($"Listening on {bindIP}:{port}.");
            var network = Network.Listen(bindIP, port);
            if (network != null)
            {
                _networks.Add(network);
                _networkLookup.Add(network.ID, network);
                if (_networks.Count == 1)
                {
                    AddHandler(this);
                    //SubscribeInterface<INetworkEvent>(OnNetworkEventEarliest, EventOrders.Earliest);
                    //if (GetGlobal("IsServer"))
                    //    SubscribeInterface<INetworkEvent>(OnNetworkEventLatest, EventOrders.Latest);
                    AddUpdate(OnUpdate);
                }

                CallEvent(new ServerStartEvent() {Network = network});

            }
            return network;
        }

        public bool TempConnect(string host, ushort port, Action<Network, NetConnection> onConnect)
        {
            var network = Connect(host, port);
            if (!network.IsCreated)
                return false;

            _tempConnects.Add(network.ID, onConnect);
            return true;
        }

        public Network GetNetwork(NetConnection connection)
        {
            _networkLookup.TryGetValue(connection.NetworkID, out var network);
            return network;
        }

        public Network GetNetwork(int networkID)
        {
            _networkLookup.TryGetValue(networkID, out var network);
            return network;
        }

        public float GetLastPing(NetConnection connection)
        {
            if (_lastPing.TryGetValue(connection, out float lastPing))
                return lastPing;
            return -1;
        }

        public float GetRoundTripTime(NetConnection connection)
        {
            if (_roundTripTimes.TryGetValue(connection, out float rtt))
                return rtt;
            return -1;
        }

        /// <summary>
        /// Sends a buffer of data to the specified recipient. This system will automatically receive the data as
        /// long as the system also exists on the recipient. This also automatically releases the provided buffer 
        /// back into the pool. To prevent this behaviour set <see cref="ByteBuffer.DoNotRelease"/> to true.
        /// </summary>
        public void Send<T>(ByteBuffer buffer, NetConnection recipient) where T : INetHandler
        {
            Send(TypeID<T>.ID, buffer, recipient);
        }

        /// <summary>
        /// Sends a buffer of data to the specified recipients. This system will automatically receive the data as
        /// long as the system also exists on the recipients. This also automatically releases the provided buffer 
        /// back into the pool. To prevent this behaviour set <see cref="ByteBuffer.DoNotRelease"/> to true.
        /// </summary>
        public void Send<T>(ByteBuffer buffer, Recipients recipients = null) where T : INetHandler
        {
            if (recipients == null)
                recipients = ServerConnection.IsCreated ? Recipients.Server : Recipients.All;

            Send(TypeID<T>.ID, buffer, recipients);
        }

        /// <summary>
        /// Sends a buffer of data to the specified recipients. This system will automatically receive the data as
        /// long as the system also exists on the recipients. This also automatically releases the provided buffer 
        /// back into the pool. To prevent this behaviour set <see cref="ByteBuffer.DoNotRelease"/> to true.
        /// </summary>
        public void Send(TypeID typeID, ByteBuffer buffer, Recipients recipients = null)
        {
            if (recipients == null)
                recipients = ServerConnection.IsCreated ? Recipients.Server : Recipients.All;

            bool prevRelease = buffer.DoNotRelease;
            buffer.DoNotRelease = true;
            foreach (var connection in recipients)
            {
                Send(typeID, buffer, connection);
            }

            buffer.DoNotRelease = prevRelease;

            InnerReleaseBuffer(buffer, false);
            Recipients.ReleaseRecipients(recipients);
        }

        /// <summary>
        /// Sends a buffer of data to the specified recipient. This system will automatically receive the data as
        /// long as the system also exists on the recipient. This also automatically releases the provided buffer 
        /// back into the pool. To prevent this behaviour set <see cref="ByteBuffer.DoNotRelease"/> to true.
        /// </summary>
        public bool Send(TypeID typeID, ByteBuffer buffer, NetConnection recipient)
        {
            try
            {
                if (!recipient.IsCreated)
                    return false;

                if (!_networkLookup.TryGetValue(recipient.NetworkID, out var network) || !network.IsCreated)
                {
                    LogError($"{typeID.Name} tried to send a network message to a network that didn't exist or is disposed. ({recipient.NetworkID})");
                    return false;
                }

                if (buffer._Length > MTU_SIZE)
                {
                    int packets = Math.DivRem(buffer._Length, MTU_SIZE, out int remainder);
                    if (remainder > 0)
                        packets++;

                    for (int i = 0; i < packets; i++)
                    {
                        int packetLength = Math.Min(MTU_SIZE, buffer._Length - i * MTU_SIZE);
                        var writer = network.BeginSend(recipient, packetLength + sizeof(ushort) + sizeof(byte) * 2 + sizeof(int));
                        if (!writer.IsCreated)
                            return false;

                        writer.WriteUShort(NET_VERSION);
                        writer.WriteByte((byte) (i + 1)); // Current Packet
                        writer.WriteByte((byte) packets); // Total Packets
                        writer.WriteInt(typeID._ID);

                        unsafe
                        {
                            fixed (byte* b = buffer._Data)
                            {
                                writer.WriteBytes(b + i * MTU_SIZE, packetLength);
                            }
                        }

                        network.EndSend(writer);
                    }
                }
                else
                {
                    var writer = network.BeginSend(recipient, buffer._Length + sizeof(ushort) + sizeof(byte) * 2 + sizeof(int));
                    if (!writer.IsCreated)
                        return false;

                    writer.WriteUShort(NET_VERSION);
                    writer.WriteByte(1); // Current Packet
                    writer.WriteByte(1); // Total Packets
                    writer.WriteInt(typeID._ID);

                    unsafe
                    {
                        fixed (byte* b = buffer._Data)
                        {
                            writer.WriteBytes(b, buffer._Length);
                        }
                    }

                    network.EndSend(writer);
                }

                return true;
            }
            finally
            {
                InnerReleaseBuffer(buffer, false);
            }
        }

        public void AddHandler<T>(T handler) where T : INetHandler
        {
            var typeID = handler.GetType().GetTypeID();
            _handlers.Add(typeID, handler);
        }

        public void RemoveHandler<T>(T handler) where T : INetHandler
        {
            var typeID = handler.GetType().GetTypeID();
            _handlers.Remove(typeID);
        }

        public bool Disconnect(NetConnection connection)
        {
            //Log($"Disconnecting connection. ({connection.Connection.InternalId})");
            var network = _networkLookup[connection.NetworkID];

            _lastPing.Remove(connection);
            _roundTripTimes.Remove(connection);

            if (network.Disconnect(connection))
            {
                _connections.Remove(connection);
                CallEvent(new ServerRemoveClientEvent()
                {
                    Network = network,
                    Connection = connection,
                    WasKicked = true,
                });
                return true;
            }

            return false;
        }

        private bool RemoveNetwork(Network network)
        {
            bool removed = _networks.Remove(network);
            _networkLookup.Remove(network.ID);
            if (_networks.Count == 0)
            {
                RemoveHandler(this);
                //UnsubscribeInterface<INetworkEvent>(OnNetworkEventEarliest, EventOrders.Earliest);
                //if (GetGlobal("IsServer"))
                //    UnsubscribeInterface<INetworkEvent>(OnNetworkEventLatest, EventOrders.Latest);
                RemoveUpdate(OnUpdate);
            }

            return removed;
        }

        public void Shutdown(Network network)
        {
            if (!network.IsCreated || _shuttingDown.Contains(network.ID))
                return;
            _shuttingDown.Add(network.ID);

            var connections = network.Connections;
            //Log($"Disconnecting {connections.Count} connections.");

            if (network.IsListening)
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    var connection = connections[i];
                    Disconnect(connection);
                }
            }

            network.Shutdown();

            if (network.IsListening)
                CallEvent(new ServerShutdownEvent() { Network = network });
            else
            {
                CallEvent(new ClientShutdownEvent() {Network = network});
                ServerConnection = default;
            }

            RemoveNetwork(network);
            _shuttingDown.Remove(network.ID);
        }

        public void Shutdown()
        {
            for (int i = 0; i < _networks.Count; i++)
            {
                var network = _networks[i];
                _shuttingDown.Add(network.ID);
                if (network.IsListening)
                    CallEvent(new ServerShutdownEvent() { Network = network });
                else
                    CallEvent(new ClientShutdownEvent() { Network = network });
                network.Shutdown();
                RemoveNetwork(network);
                _shuttingDown.Remove(network.ID);
            }

            ServerConnection = default;
            _connections.Clear();
            _roundTripTimes.Clear();
            _lastPing.Clear();
            _networks.Clear();
            _networkLookup.Clear();
            _handlers.Clear();
            RemoveUpdate(OnUpdate);
        }

        public NetworkEndPoint GetRemoteEndPoint(NetConnection connection)
        {
            if (_networkLookup.TryGetValue(connection.NetworkID, out var network))
            {
                return network.GetRemoteEndPoint(connection);
            }

            return default;
        }
    }
}
