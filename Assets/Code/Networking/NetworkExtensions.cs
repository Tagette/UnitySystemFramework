using UnitySystemFramework.Core;

namespace UnitySystemFramework.Networking
{
    public static class NetworkExtensions
    {
        /// <summary>
        /// Gets the network mode for this event <see cref="NetworkMode"/>.
        /// </summary>
        public static NetworkMode GetNetworkMode<T>(this T evt) where T : struct, INetworkEvent
        {
            evt.GetComponent(out NetworkEventComponent component);
            return component.Mode;
        }

        /// <inheritdoc cref="GetNetworkMode{T}(T)"/>
        public static NetworkMode GetNetworkMode(this INetworkEvent evt)
        {
            evt.GetComponent(out NetworkEventComponent component);
            return component.Mode;
        }

        /// <summary>
        /// Sets the network mode for this event <see cref="NetworkMode"/>.
        /// </summary>
        public static T SetNetworkMode<T>(this T evt, NetworkMode mode) where T : struct, INetworkEvent
        {
            evt.GetComponent(out NetworkEventComponent component);
            component.Mode = mode;
            evt.SetComponent(component);

            return evt;
        }

        /// <inheritdoc cref="SetNetworkMode{T}(T,NetworkMode)"/>
        public static INetworkEvent SetNetworkMode(this INetworkEvent evt, NetworkMode mode)
        {
            evt.GetComponent(out NetworkEventComponent component);
            component.Mode = mode;
            evt.SetComponent(component);

            return evt;
        }

        /// <summary>
        /// Gets the recipients of this event.
        /// </summary>
        public static Recipients GetRecipients<T>(this T evt) where T : struct, INetworkEvent
        {
            evt.GetComponent(out NetworkEventComponent component);
            return component.Recipients;
        }

        /// <inheritdoc cref="GetRecipients{T}(T)"/>
        public static Recipients GetRecipients(this INetworkEvent evt)
        {
            evt.GetComponent(out NetworkEventComponent component);
            return component.Recipients;
        }

        /// <summary>
        /// Sets the recipients of this event.
        /// </summary>
        public static T SetRecipients<T>(this T evt, Recipients recipients) where T : struct, INetworkEvent
        {
            evt.ClearComponents();
            evt.GetComponent(out NetworkEventComponent component);
            component.Recipients = recipients;
            evt.SetComponent(component);

            return evt;
        }

        /// <inheritdoc cref="SetRecipients{T}(T,Recipients)"/>
        public static INetworkEvent SetRecipients(this INetworkEvent evt, Recipients recipients)
        {
            evt.GetComponent(out NetworkEventComponent component);
            component.Recipients = recipients;
            evt.SetComponent(component);

            return evt;
        }

        /// <summary>
        /// Gets the sender of this event.
        /// </summary>
        public static NetConnection GetSender<T>(this T evt) where T : struct, INetworkEvent
        {
            evt.GetComponent(out NetworkEventComponent component);
            return component.Sender;
        }

        /// <inheritdoc cref="GetSender{T}(T)"/>
        public static NetConnection GetSender(this INetworkEvent evt)
        {
            evt.GetComponent(out NetworkEventComponent component);
            return component.Sender;
        }

        /// <summary>
        /// Determines if the sender of this event was the server.
        /// </summary>
        public static bool WasReceivedFromServer<T>(this T evt) where T : struct, INetworkEvent
        {
            evt.GetComponent(out NetworkEventComponent component);
            return !Game.CurrentGame.GetGlobal("IsServer") && component.Sender.IsCreated;
        }

        /// <inheritdoc cref="WasReceivedFromServer{T}(T)"/>
        public static bool WasReceivedFromServer(this INetworkEvent evt)
        {
            evt.GetComponent(out NetworkEventComponent component);
            return !Game.CurrentGame.GetGlobal("IsServer") && component.Sender.IsCreated;
        }

        /// <summary>
        /// Determines if the sender of this event was the server.
        /// </summary>
        public static bool WasReceived<T>(this T evt) where T : struct, INetworkEvent
        {
            evt.GetComponent(out NetworkEventComponent component);
            return component.Sender.IsCreated;
        }

        /// <inheritdoc cref="WasReceived{T}(T)"/>
        public static bool WasReceived(this INetworkEvent evt)
        {
            evt.GetComponent(out NetworkEventComponent component);
            return component.Sender.IsCreated;
        }
    }
}
