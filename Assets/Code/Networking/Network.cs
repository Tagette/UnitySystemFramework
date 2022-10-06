using UnitySystemFramework.Core;
using System.Collections.Generic;
using Unity.Networking.Transport;

namespace UnitySystemFramework.Networking
{
    public class Network
    {
        private static int _nextID;
        public static Network Connect(string host, ushort port)
        {
            var driver = NetworkDriver.Create(new NetworkConfigParameter
            {
                maxConnectAttempts = 10,
                connectTimeoutMS = 300,
                disconnectTimeoutMS = NetworkParameterConstants.DisconnectTimeoutMS,
                maxFrameTimeMS = 0
            });

            var pipeline = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            var ep = NetworkEndPoint.Parse(host, port);
            var connection = driver.Connect(ep);

            if (connection.IsCreated)
            {
                var network = new Network(++_nextID, driver, pipeline);
                network.IP = host;
                network.Port = port;
                return network;
            }

            driver.Dispose();
            return null;
        }

        public static Network Listen(string bindIP, ushort port)
        {
            var driver = NetworkDriver.Create(new NetworkConfigParameter
            {
                maxConnectAttempts = 10,
                connectTimeoutMS = 300,
                disconnectTimeoutMS = NetworkParameterConstants.DisconnectTimeoutMS,
                maxFrameTimeMS = 0
            });

            var pipeline = driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            var ep = NetworkEndPoint.Parse(bindIP, port);

            if (driver.Bind(ep) != 0)
            {
                driver.Dispose();
                return null;
            }

            if (driver.Listen() != 0)
            {
                driver.Dispose();
                return null;
            }

            var network = new Network(++_nextID, driver, pipeline);
            network.IP = bindIP;
            network.Port = port;
            network.IsListening = true;
            return network;
        }

        private NetworkDriver _driver;
        private readonly NetworkPipeline _pipeline;
        private readonly List<NetConnection> _connections = new List<NetConnection>();
        private bool _shuttingDown;

        private Network(int id, NetworkDriver driver, NetworkPipeline pipeline)
        {
            ID = id;
            _driver = driver;
            _pipeline = pipeline;

            Connections = _connections.AsReadOnly();
        }

        public bool IsCreated => _driver.IsCreated;
        public bool IsListening { get; private set; }
        public IReadOnlyList<NetConnection> Connections { get; }

        public int ID { get; private set; }
        public string IP { get; private set; }
        public ushort Port { get; private set; }

        public void Update()
        {
            if (!_driver.IsCreated)
                return;
            _driver.ScheduleUpdate().Complete();
        }

        public bool Accept(out NetConnection connection)
        {
            connection = default;

            if (!_driver.IsCreated)
                return false;

            var conn = _driver.Accept();
            if (!conn.IsCreated)
                return false;

            if (_driver.ReceiveErrorCode != 0)
                Game.LogError($"ReceiveError: {_driver.ReceiveErrorCode}");

            connection = new NetConnection()
            {
                Connection = conn,
                NetworkID = ID,
            };
            _connections.Add(connection);
            return true;
        }

        public NetworkEvent.Type PopEvent(out NetConnection connection, out DataStreamReader reader)
        {
            var type = _driver.PopEvent(out var conn, out reader);

            if (_driver.ReceiveErrorCode != 0)
                Game.LogError($"ReceiveError: {_driver.ReceiveErrorCode}");

            connection = new NetConnection()
            {
                NetworkID = ID,
                Connection = conn,
            };

            if (type == NetworkEvent.Type.Connect)
                _connections.Add(connection);
            else if (type == NetworkEvent.Type.Disconnect)
                _connections.Remove(connection);

            return type;
        }

        public DataStreamWriter BeginSend(NetConnection connection, int length = 0)
        {
            if (!_driver.IsCreated || _driver.GetConnectionState(connection.Connection) != NetworkConnection.State.Connected)
                return default;

            return _driver.BeginSend(_pipeline, connection.Connection, length);
        }

        public void EndSend(DataStreamWriter writer)
        {
            if (!writer.IsCreated)
                return;

            _driver.EndSend(writer);
        }

        public void AbortSend(DataStreamWriter writer)
        {
            if (!writer.IsCreated)
                return;

            _driver.AbortSend(writer);
        }

        public bool Disconnect(NetConnection connection)
        {
            if (!_connections.Remove(connection))
                return false;
            if (connection.IsCreated)
            {
                _driver.ScheduleFlushSend(default).Complete();
                _driver.Disconnect(connection.Connection);
                _driver.ScheduleFlushSend(default).Complete();
            }

            return true;
        }

        public void Shutdown()
        {
            if (!IsCreated || _shuttingDown)
                return;
            _shuttingDown = true;

            // There's, apparently, no way to check if this is allowed right now...
            // This can sometimes throw NullReferenceException's when called on shutdown.
            try { _driver.ScheduleFlushSend(default).Complete(); } catch { }

            for (int i = 0; i < _connections.Count; i++)
            {
                var connection = _connections[i];
                if (connection.IsCreated)
                    _driver.Disconnect(connection.Connection);
            }

            try { _driver.ScheduleFlushSend(default).Complete(); } catch { }

            _driver.Dispose();
            _driver = default;
            _connections.Clear();

            IsListening = false;
            _shuttingDown = false;
        }

        public NetworkEndPoint GetRemoteEndPoint(NetConnection connection)
        {
            return _driver.RemoteEndPoint(connection.Connection);
        }
    }
}
