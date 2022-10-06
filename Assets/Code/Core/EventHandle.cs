using System;

namespace UnitySystemFramework.Core
{
    public readonly struct EventHandle : IEquatable<EventHandle>
    {
        public static EventHandle NoEvent = default;

        private readonly ulong _eventID;
        private readonly TypeID _type;
        private readonly Action _invoker;
        private readonly Func<IEvent> _getter;

        public EventHandle(ulong eventID, TypeID type, Action invoker, Func<IEvent> getter)
        {
            _eventID = eventID;
            _type = type;
            _invoker = invoker;
            _getter = getter;
        }

        public TypeID Type => _type;

        /// <summary>
        /// Determines whether or not this handle represents an event or not.
        /// </summary>
        public bool IsAnEvent => _eventID != default;

        /// <summary>
        /// Gets the current event. This call will provide you a boxed version of the event.
        /// </summary>
        public IEvent GetEvent()
        {
            return _getter?.Invoke();
        }

        /// <summary>
        /// Calls the event. This will do nothing if the event is currently being called.
        /// </summary>
        public void Invoke()
        {
            try
            {
                _invoker?.Invoke();
            }
            catch (Exception ex)
            {
                Game.LogException(ex);
            }
        }

        public bool Equals(EventHandle other)
        {
            return _eventID == other._eventID;
        }

        public override bool Equals(object obj)
        {
            return obj is EventHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _eventID.GetHashCode();
        }

        public static bool operator ==(EventHandle a, EventHandle b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(EventHandle a, EventHandle b)
        {
            return !a.Equals(b);
        }
    }
}
