using UnitySystemFramework.Core;
using UnitySystemFramework.Serialization;

namespace UnitySystemFramework.Networking
{
    public enum NetworkMode : byte
    {
        /// <summary>
        /// A mode where the event is first send to the server, then to the clients afterwards and handled. This
        /// means you feel the full round trip latency before your event is called. If called by the server, it's
        /// just sent to the clients after the server handles it.
        /// </summary>
        FullAuthoritative,

        /// <summary>
        /// A mode where the client first handles the event before sending the event to the server. The server can
        /// then take action and correct the player if needed. The event (by default) is then sent to the original
        /// sender to be handled again but in Authoritative mode. This allows for immediate feedback on the client
        /// while still being authoritative and secure.
        /// </summary>
        ClientPrediction,
    }

    /// <summary>
    /// An event that can be networked.
    /// </summary>
    public interface INetworkEvent : ICompositeEvent
    {
        /// <summary>
        /// Called when the event is being read from the buffer.
        /// </summary>
        /// <param name="game">An instance to the current game.</param>
        /// <param name="buffer">The buffer to read from.</param>
        void OnRead(IGame game, ByteBuffer buffer);

        /// <summary>
        /// Called when the event is being written to the buffer.
        /// </summary>
        /// <param name="game">An instance to the current game.</param>
        /// <param name="buffer">The buffer to write to.</param>
        void OnWrite(IGame game, ByteBuffer buffer);
    }

    /// <summary>
    /// An event component that can be networked.
    /// </summary>
    public interface INetworkEventComponent : IEventComponent
    {
        /// <summary>
        /// Called when the event is being read from the buffer.
        /// </summary>
        /// <param name="game">An instance to the current game.</param>
        /// <param name="buffer">The buffer to read from.</param>
        void OnRead(IGame game, ByteBuffer buffer);

        /// <summary>
        /// Called when the event is being written to the buffer.
        /// </summary>
        /// <param name="game">An instance to the current game.</param>
        /// <param name="buffer">The buffer to write to.</param>
        void OnWrite(IGame game, ByteBuffer buffer);
    }

    /// <summary>
    /// A network event component that stores all of the data related to a network event.
    /// </summary>
    public struct NetworkEventComponent : IEventComponent
    {
        /// <summary>
        /// The connection of the sender of the event.
        /// </summary>
        public NetConnection Sender { get; set; }

        /// <summary>
        /// The recipients to send the event to. The server may set this before calling or while handling the event. 
        /// The client can only ever send to the server. See the <see cref="Networking.Recipients" /> class for possible values.
        /// </summary>
        public Recipients Recipients { get; set; }

        /// <summary>
        /// The network mode for the event. <see cref="NetworkMode"/>
        /// </summary>
        public NetworkMode Mode { get; set; }
    }
}
