using System;
using System.Collections.Generic;

namespace UnitySystemFramework.Core
{
    public delegate void EventSubMethod<T>(ref T e) where T : struct, IEvent;

    public delegate void EventInterfaceMethod<T>(ref T e) where T : IEvent;
    
    public partial interface IGame
    {
        /// <summary>
        /// Provides an event handle to the current invoking event.
        /// </summary>
        EventHandle CurrentEvent { get; }

        /// <summary>
        /// Gets the current event order for an even that is being handled right now.
        /// </summary>
        EventOrders CurrentEventOrder { get; }

        /// <summary>
        /// Subscribes the provided method to the event type given. When the event is called, this method will be invoked.
        /// </summary>
        void Subscribe<T>(EventSubMethod<T> subscriber, EventOrders orders = EventOrders.Normal) where T : struct, IEvent;

        /// <summary>
        /// Unsubscribes a method from being invoked by the specified type of event.
        /// </summary>
        void Unsubscribe<T>(EventSubMethod<T> subscriber) where T : struct, IEvent;

        /// <summary>
        /// Unsubscribes a method from being invoked by the specified type of event. This removes an handler with the
        /// specific event order provided.
        /// </summary>
        void Unsubscribe<T>(EventSubMethod<T> subscriber, EventOrders orders) where T : struct, IEvent;

        /// <summary>
        /// Subscribes the provided method to be invoked when any event implementing the provided interface type is called.
        /// </summary>
        void SubscribeInterface<T>(EventInterfaceMethod<T> subscriber, EventOrders orders = EventOrders.Normal) where T : IEvent;

        /// <summary>
        /// Unsubscribes the provided method from being invoked when any event implementing the provided interface type is called.
        /// </summary>
        void UnsubscribeInterface<T>(EventInterfaceMethod<T> subscriber) where T : IEvent;

        /// <summary>
        /// Unsubscribes the provided method from being invoked when any event implementing the provided interface type is called.
        /// </summary>
        void UnsubscribeInterface<T>(EventInterfaceMethod<T> subscriber, EventOrders orders) where T : IEvent;

        /// <summary>
        /// Calls/Raises the provided event so that any methods that are subscribed are invoked.
        /// </summary>
        T CallEvent<T>(T evt) where T : struct, IEvent;

        /// <summary>
        /// When called, the current invoking event is cancelled and no more subscribers will be invoked. An event
        /// handle is returned which allows you to invoke the event at a later time using <see cref="EventHandle.Invoke()"/>.
        /// </summary>
        EventHandle DelayCurrentEvent();

        /// <summary>
        /// When called, the current invoking event will be cancelled and no more subscribers will be invoked.
        /// </summary>
        void CancelCurrentEvent();
    }

    public static partial class Game
    {
    }

    public abstract partial class BaseGame
    {
        private delegate void InvokeInterfaceEventMethod(ref IEvent e);

        private class EventSubGroup
        {
            public Delegate Earliest;
            public Delegate Early;
            public Delegate Normal;
            public Delegate Late;
            public Delegate Latest;
            public readonly Dictionary<Delegate, EventSubInfo> Subscribers = new Dictionary<Delegate, EventSubInfo>();
        }

        private struct EventSubInfo : IEquatable<EventSubInfo>
        {
            public Delegate Subscriber;
            public Delegate Invoker;
            public EventOrders Orders;

            public EventSubInfo(Delegate subscriber, EventOrders orders)
            {
                Subscriber = subscriber;
                Invoker = null;
                Orders = orders;
            }

            public EventSubInfo(Delegate subscriber, Delegate invoker, EventOrders orders)
            {
                Subscriber = subscriber;
                Invoker = invoker;
                Orders = orders;
            }

            public bool Equals(EventSubInfo other)
            {
                return Equals(Subscriber, other.Subscriber);
            }

            public override bool Equals(object obj)
            {
                return obj is EventSubInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (Subscriber != null ? Subscriber.GetHashCode() : 0);
            }
        }

        private class SystemEventSubGroup
        {
            public SystemEventSubGroup(TypeID systemID)
            {
                SystemID = systemID;
            }

            public readonly TypeID SystemID;
            public readonly HashSet<(TypeID, Delegate)> Subs = new HashSet<(TypeID, Delegate)>();
        }

        private readonly Dictionary<TypeID, EventSubGroup> _eventSubs = new Dictionary<TypeID, EventSubGroup>();
        private readonly Dictionary<TypeID, SystemEventSubGroup> _systemEventSubs = new Dictionary<TypeID, SystemEventSubGroup>();

        private ulong _lastEventID;
        private bool _cancelEvent;

        private EventHandle _currentEvent;
        private EventOrders _currentEventOrder;

        /// <inheritdoc cref="IGame.CurrentEvent"/>
        EventHandle IGame.CurrentEvent => _currentEvent;

        /// <inheritdoc cref="IGame.CurrentEventOrder"/>
        EventOrders IGame.CurrentEventOrder => _currentEventOrder;

        /// <inheritdoc cref="IGame.Subscribe{T}(EventSubMethod{T}, EventOrders)"/>
        void IGame.Subscribe<T>(EventSubMethod<T> subscriber, EventOrders orders)
        {
            void SafeInvoke(ref T e)
            {
                try
                {
                    if (_currentSystem == null || _currentSystem.Enabled)
                        subscriber(ref e);
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }

            var typeID = TypeID<T>.ID;
            var invoker = (EventSubMethod<T>)SafeInvoke;
            Subscribe(typeID, subscriber, invoker, orders);
        }

        /// <inheritdoc cref="IGame.Unsubscribe{T}(EventSubMethod{T})"/>
        void IGame.Unsubscribe<T>(EventSubMethod<T> subscriber)
        {
            var typeID = TypeID<T>.ID;
            Unsubscribe(typeID, subscriber, 0);
        }

        /// <inheritdoc cref="IGame.Unsubscribe{T}(EventSubMethod{T}, EventOrders)"/>
        void IGame.Unsubscribe<T>(EventSubMethod<T> subscriber, EventOrders orders)
        {
            var typeID = TypeID<T>.ID;
            Unsubscribe(typeID, subscriber, orders);
        }

        /// <inheritdoc cref="IGame.SubscribeInterface{T}(EventInterfaceMethod{T}, EventOrders)"/>
        void IGame.SubscribeInterface<T>(EventInterfaceMethod<T> subscriber, EventOrders orders)
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException("This method only subscribes to interface types.", "T");

            void InterfaceInvoke(ref IEvent e)
            {
                var evt = (T)e;
                try
                {
                    if (_currentSystem == null || _currentSystem.Enabled)
                    {
                        subscriber(ref evt);
                        e = evt;
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }

            var typeID = TypeID<T>.ID;
            var invoker = (InvokeInterfaceEventMethod)InterfaceInvoke;
            Subscribe(typeID, subscriber, invoker, orders);
        }

        /// <inheritdoc cref="IGame.UnsubscribeInterface{T}(EventInterfaceMethod{T})"/>
        void IGame.UnsubscribeInterface<T>(EventInterfaceMethod<T> subscriber)
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException("This method only unsubscribes to interface types.", "T");

            var typeID = TypeID<T>.ID;
            Unsubscribe(typeID, subscriber, 0);
        }

        /// <inheritdoc cref="IGame.UnsubscribeInterface{T}(EventInterfaceMethod{T}, EventOrders)"/>
        void IGame.UnsubscribeInterface<T>(EventInterfaceMethod<T> subscriber, EventOrders orders)
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException("This method only unsubscribes to interface types.", "T");

            var typeID = TypeID<T>.ID;
            Unsubscribe(typeID, subscriber, orders);
        }

        private void Subscribe(TypeID typeID, Delegate subscriber, Delegate invoker, EventOrders orders)
        {
            BeginSample("BaseGame.Subscribe()");
            // TODO: Only create this if we actually decide to keep it.
            if (!_eventSubs.TryGetValue(typeID, out var group))
                _eventSubs[typeID] = group = new EventSubGroup();

            if (!group.Subscribers.TryGetValue(subscriber, out var info))
                info = new EventSubInfo(subscriber, invoker, 0);

            if ((info.Orders & EventOrders.Earliest) == 0 && (orders & EventOrders.Earliest) != 0)
                group.Earliest = Delegate.Combine(group.Earliest, invoker);
            if ((info.Orders & EventOrders.Early) == 0 && (orders & EventOrders.Early) != 0)
                group.Early = Delegate.Combine(group.Early, invoker);
            if ((info.Orders & EventOrders.Normal) == 0 && (orders & EventOrders.Normal) != 0)
                group.Normal = Delegate.Combine(group.Normal, invoker);
            if ((info.Orders & EventOrders.Late) == 0 && (orders & EventOrders.Late) != 0)
                group.Late = Delegate.Combine(group.Late, invoker);
            if ((info.Orders & EventOrders.Latest) == 0 && (orders & EventOrders.Latest) != 0)
                group.Latest = Delegate.Combine(group.Latest, invoker);

            info.Orders |= orders;
            group.Subscribers[subscriber] = info;

            // TODO: Only add system specific events inside of OnInit() and OnEnd() because an event might be
            // TODO: subscribed to the wrong system if the subscription occurs during another system's callback. 
            if (_currentSystem?.System != null)
            {
                var systemID = _currentSystem.TypeID;
                if (!_systemEventSubs.TryGetValue(systemID, out var systemSub))
                    _systemEventSubs[systemID] = systemSub = new SystemEventSubGroup(systemID);

                systemSub.Subs.Add((typeID, subscriber));
            }
            EndSample();
        }

        private void Unsubscribe(TypeID typeID, Delegate subscriber, EventOrders orders)
        {
            BeginSample("BaseGame.Unsubscribe()");
            if (_eventSubs.TryGetValue(typeID, out var group))
            {
                if (group.Subscribers.TryGetValue(subscriber, out var info))
                {
                    if (orders == 0 || (orders & EventOrders.Earliest) != 0)
                        group.Earliest = Delegate.Remove(group.Earliest, info.Invoker);
                    if (orders == 0 || (orders & EventOrders.Early) != 0)
                        group.Early = Delegate.Remove(group.Early, info.Invoker);
                    if (orders == 0 || (orders & EventOrders.Normal) != 0)
                        group.Normal = Delegate.Remove(group.Normal, info.Invoker);
                    if (orders == 0 || (orders & EventOrders.Late) != 0)
                        group.Late = Delegate.Remove(group.Late, info.Invoker);
                    if (orders == 0 || (orders & EventOrders.Latest) != 0)
                        group.Latest = Delegate.Remove(group.Latest, info.Invoker);

                    if (orders == 0)
                        info.Orders = 0;
                    else
                        info.Orders = (info.Orders & ~orders);

                    if (info.Orders == 0)
                        group.Subscribers.Remove(subscriber); // Remove if all orders have been removed.
                    else
                        group.Subscribers[subscriber] = info; // Update the new orders.

                    if (group.Subscribers.Count == 0)
                        _eventSubs.Remove(typeID);
                }
            }

            // TODO: Only remove system specific events inside of OnInit() and OnEnd() because an event might be
            // TODO: subscribed to the wrong system if the subscription occurs during another system's callback. 
            if (_currentSystem?.System != null)
            {
                var systemID = _currentSystem.TypeID;
                if (_systemEventSubs.TryGetValue(systemID, out var systemGroup))
                {
                    var systemSub = (typeID, subscriber);
                    systemGroup.Subs.Remove(systemSub);

                    if (systemGroup.Subs.Count == 0)
                        _systemEventSubs.Remove(systemID);
                }
            }
            EndSample();
        }

        /// <inheritdoc cref="IGame.CallEvent{T}(T)"/>
        T IGame.CallEvent<T>(T evt)
        {
            using (BeginSample("BaseGame.CallEvent()"))
            {
                _cancelEvent = false;
                _lastEventID++;

                EventHandle thisEvent = default;
                thisEvent = new EventHandle(_lastEventID, TypeID<T>.ID, () =>
                {
                    if (Equals(_currentEvent, thisEvent))
                        return;

                    thisGame.CallEvent(evt);
                }, () => evt);

                var prevEvent = SetCurrentEvent(thisEvent);
                var prevGame = Game.SetGame(this);

                var typeID = evt.GetType().GetTypeID();
                var interfaces = Reflect.GetInterfaces(typeID);
                var interfaceGroups = new EventSubGroup[interfaces.Count];

                for (int i = 0; i < interfaces.Count; i++)
                {
                    var faceID = interfaces[i];
                    _eventSubs.TryGetValue(faceID, out var iGroup);
                    interfaceGroups[i] = iGroup;
                }

                _eventSubs.TryGetValue(typeID, out var group);

                using (BeginSample("Earliest"))
                {
                    if(group != null)
                        HandleOrder(ref evt, group.Earliest, EventOrders.Earliest, typeID.Name);
                    for (int i = 0; i < interfaces.Count; i++)
                    {
                        var faceID = interfaces[i];
                        EventSubGroup iGroup;
                        if ((iGroup = interfaceGroups[i]) != null)
                            HandleInterfaceOrder(ref evt, iGroup.Earliest, EventOrders.Earliest, faceID.Name);
                    }
                }

                using (BeginSample("Early"))
                {
                    if (group != null)
                        HandleOrder(ref evt, group.Early, EventOrders.Early, typeID.Name);
                    for (int i = 0; i < interfaces.Count; i++)
                    {
                        var faceID = interfaces[i];
                        EventSubGroup iGroup;
                        if ((iGroup = interfaceGroups[i]) != null)
                            HandleInterfaceOrder(ref evt, iGroup.Early, EventOrders.Early, faceID.Name);
                    }
                }

                using (BeginSample("Normal"))
                {
                    if (group != null)
                        HandleOrder(ref evt, group.Normal, EventOrders.Normal, typeID.Name);
                    for (int i = 0; i < interfaces.Count; i++)
                    {
                        var faceID = interfaces[i];
                        EventSubGroup iGroup;
                        if ((iGroup = interfaceGroups[i]) != null)
                            HandleInterfaceOrder(ref evt, iGroup.Normal, EventOrders.Normal, faceID.Name);
                    }
                }

                using (BeginSample("Late"))
                {
                    if (group != null)
                        HandleOrder(ref evt, group.Late, EventOrders.Late, typeID.Name);
                    for (int i = 0; i < interfaces.Count; i++)
                    {
                        var faceID = interfaces[i];
                        EventSubGroup iGroup;
                        if ((iGroup = interfaceGroups[i]) != null)
                            HandleInterfaceOrder(ref evt, iGroup.Late, EventOrders.Late, faceID.Name);
                    }
                }

                using (BeginSample("Latest"))
                {
                    if (group != null)
                        HandleOrder(ref evt, group.Latest, EventOrders.Latest, typeID.Name);
                    for (int i = 0; i < interfaces.Count; i++)
                    {
                        var faceID = interfaces[i];
                        EventSubGroup iGroup;
                        if ((iGroup = interfaceGroups[i]) != null)
                            HandleInterfaceOrder(ref evt, iGroup.Latest, EventOrders.Latest, faceID.Name);
                    }
                }

                SetCurrentEvent(prevEvent);
                Game.SetGame(prevGame);

                if (evt is ICompositeEvent composite)
                    composite.ClearComponents();
            }

            return evt;
        }

        private void HandleOrder<T>(ref T evt, Delegate orderDele, EventOrders order, string sample) where T : struct, IEvent
        {
            if (!_cancelEvent && orderDele != null)
            {
                BeginSample(sample);
                var prevOrder = SetCurrentEventOrder(order);
                var invoker = (EventSubMethod<T>)orderDele;
                invoker(ref evt);
                SetCurrentEventOrder(prevOrder);
                EndSample();
            }
        }

        private void HandleInterfaceOrder<T>(ref T evt, Delegate orderDele, EventOrders order, string sample) where T : IEvent
        {
            if (!_cancelEvent && orderDele != null)
            {
                // Yes this boxes.
                IEvent iEvt = evt;
                BeginSample(sample);
                var prevOrder = SetCurrentEventOrder(order);
                var invoker = (InvokeInterfaceEventMethod)orderDele;
                invoker(ref iEvt);
                evt = (T)iEvt;
                SetCurrentEventOrder(prevOrder);
                EndSample();
            }
        }

        /// <inheritdoc cref="IGame.DelayCurrentEvent()"/>
        EventHandle IGame.DelayCurrentEvent()
        {
            var currentEvent = _currentEvent;
            thisGame.CancelCurrentEvent();
            return currentEvent;
        }

        /// <inheritdoc cref="IGame.CancelCurrentEvent()"/>
        void IGame.CancelCurrentEvent()
        {
            _cancelEvent = true;
        }

        private EventHandle SetCurrentEvent(EventHandle handle)
        {
            var previous = _currentEvent;
            _currentEvent = handle;
            return previous;
        }

        private EventOrders SetCurrentEventOrder(EventOrders order)
        {
            var previous = _currentEventOrder;
            _currentEventOrder = order;
            return previous;
        }
    }
}
