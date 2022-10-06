using System;
using System.Collections.Generic;

namespace UnitySystemFramework.Collections
{
    public class CapacityPool<T>
    {
        public const int MIN_CAPACITY = 4;
        public const int DEFAULT_MAX = 100;

        private class Pool<T2>
        {
            public Pool(int capacity, int max)
            {
                Capacity = capacity;
                MaxCount = max;
            }

            public readonly int Capacity;
            public int MaxCount = 100;
            public readonly Stack<T2> Stack = new Stack<T2>();

            public int Count => Stack.Count;

            public T2 Pop() => Stack.Pop();

            public void Push(T2 value) => Stack.Push(value);
        }

        private int _minRoundedCapacity = MIN_CAPACITY;
        private int _defaultMax = DEFAULT_MAX;
        private readonly Dictionary<int, Pool<T>> _pool = new Dictionary<int, Pool<T>>();
        private readonly Func<int, T> _instantiate;
        private readonly Func<T, int> _getCapacity;
        private readonly Action<T> _reset;

        public CapacityPool(Func<int, T> instantiate, Func<T, int> getCapacity, Action<T> reset = null)
        {
            _instantiate = instantiate;
            _getCapacity = getCapacity;
            _reset = reset;
        }

        public void SetMinCapacity(int capacity)
        {
            _minRoundedCapacity = MIN_CAPACITY;
            _minRoundedCapacity = GetCeilCapacity(capacity);
        }

        public void SetDefaultMax(int max)
        {
            _defaultMax = max;
        }

        public void SetMax(int max)
        {
            foreach (var pool in _pool)
            {
                pool.Value.MaxCount = max;
            }
        }

        public void SetMax(int capacity, int max)
        {
            capacity = GetCeilCapacity(capacity);
            if (!_pool.TryGetValue(capacity, out var pool))
                _pool[capacity] = pool = new Pool<T>(capacity, max);

            pool.MaxCount = max;
        }

        public T Get(int capacity)
        {
            int rounded = GetCeilCapacity(capacity);
            if (!_pool.TryGetValue(rounded, out var pool) || pool.Count == 0)
                return _instantiate(rounded);
            return pool.Pop();
        }

        public void Release(T value)
        {
            if (value == null)
                return;

            var rounded = GetFloorCapacity(_getCapacity(value));
            if (!_pool.TryGetValue(rounded, out var pool))
                _pool[rounded] = pool = new Pool<T>(rounded, _defaultMax);
            if (pool.Count < pool.MaxCount)
            {
                _reset?.Invoke(value);
                pool.Push(value);
            }
        }

        public T Instantiate(int capacity)
        {
            return _instantiate(capacity);
        }

        public int GetCapacity(T value)
        {
            return _getCapacity(value);
        }

        public int GetCeilCapacity(T value)
        {
            return GetCeilCapacity(_getCapacity(value));
        }

        public int GetCeilCapacity(int capacity)
        {
            // TODO: May not support all the way up to int.MaxValue.
            int rounded = _minRoundedCapacity;
            while(rounded > 0)
            {
                if (capacity <= rounded)
                    return rounded;

                rounded *= 4;
            }

            // This should not be possible.
            throw new ArgumentOutOfRangeException("capacity", "The given capacity is too large.");
        }

        public int GetFloorCapacity(T value)
        {
            return GetFloorCapacity(_getCapacity(value));
        }

        public int GetFloorCapacity(int capacity)
        {
            // TODO: May not support all the way up to int.MaxValue.
            int rounded = _minRoundedCapacity;
            while (rounded * 4 > 0)
            {
                if (capacity < rounded * 4)
                    return rounded;

                rounded *= 4;
            }

            // This should not be possible.
            throw new ArgumentOutOfRangeException("capacity", "The given capacity is too large.");
        }

        public void Clear()
        {
            _pool.Clear();
        }
    }
}
