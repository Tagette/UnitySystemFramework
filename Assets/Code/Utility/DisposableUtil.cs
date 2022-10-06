using System;

namespace UnitySystemFramework.Utility
{
    public readonly struct DisposableResult : IDisposable
    {
        private readonly Action _onDispose;

        public DisposableResult(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose?.Invoke();
        }
    }

    public struct DisposableResult<T> : IDisposable
    {
        public T Value;
        private readonly Action _onDispose;

        public DisposableResult(T value, Action onDispose)
        {
            Value = value;
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose?.Invoke();
        }

        public static implicit operator T(DisposableResult<T> result)
        {
            return result.Value;
        }
    }

    public struct DisposableResult<T1, T2> : IDisposable
    {
        public T1 First;
        public T2 Second;
        private readonly Action _onDispose;

        public DisposableResult(T1 first, T2 second, Action onDispose)
        {
            First = first;
            Second = second;
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose?.Invoke();
        }

        public static implicit operator (T1, T2)(DisposableResult<T1, T2> result)
        {
            return (result.First, result.Second);
        }
    }
}
