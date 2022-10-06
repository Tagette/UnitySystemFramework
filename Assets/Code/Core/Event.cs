using System;

namespace UnitySystemFramework.Core
{
    public struct Event<T> where T : struct, IEvent
    {
        private Action _unsubscribe;

        public void Subscribe(EventSubMethod<T> sub)
        {
            var game = Game.CurrentGame;
            game.Subscribe(sub);
            _unsubscribe += () => game.Unsubscribe(sub);
        }

        public void Unsubscribe(EventSubMethod<T> sub)
        {
            var game = Game.CurrentGame;
            game.Unsubscribe(sub);
            // Cannot remove sub from _unsubscribe. Not a problem to call anyways if not subscribed.
        }

        public void UnsubscribeAll()
        {
            _unsubscribe?.Invoke();
            _unsubscribe = null;
        }

        public static Event<T> operator +(Event<T> a, EventSubMethod<T> b)
        {
            a.Subscribe(b);

            return a;
        }

        public static Event<T> operator -(Event<T> a, EventSubMethod<T> b)
        {
            a.Unsubscribe(b);

            return a;
        }

        public static implicit operator Event<T>(EventSubMethod<T> sub)
        {
            Event<T> a = default;
            a.Subscribe(sub);
            return a;
        }
    }

    public struct EventAll<T> where T : class, IEvent
    {
        private Action _unsubscribe;

        public void Subscribe(EventInterfaceMethod<T> sub)
        {
            var game = Game.CurrentGame;
            game.SubscribeInterface(sub);
            _unsubscribe += () => game.UnsubscribeInterface(sub);
        }

        public void Unsubscribe(EventInterfaceMethod<T> sub)
        {
            var game = Game.CurrentGame;
            game.UnsubscribeInterface(sub);
            // Cannot remove sub from _unsubscribe. Not a problem to call anyways if not subscribed.
        }

        public void UnsubscribeAll()
        {
            _unsubscribe?.Invoke();
            _unsubscribe = null;
        }

        public static EventAll<T> operator +(EventAll<T> a, EventInterfaceMethod<T> b)
        {
            a.Subscribe(b);

            return a;
        }

        public static EventAll<T> operator -(EventAll<T> a, EventInterfaceMethod<T> b)
        {
            a.Unsubscribe(b);

            return a;
        }

        public static implicit operator EventAll<T>(EventInterfaceMethod<T> sub)
        {
            EventAll<T> a = default;
            a.Subscribe(sub);
            return a;
        }
    }
}
