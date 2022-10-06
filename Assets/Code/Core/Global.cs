using System;

namespace UnitySystemFramework.Core
{
    public struct Global
    {
        public object Value;

        public bool ToBool() => To<bool>();
        public sbyte ToSByte() => To<sbyte>();
        public byte ToByte() => To<byte>();
        public short ToShort() => To<short>();
        public ushort ToUShort() => To<ushort>();
        public int ToInt() => To<int>();
        public uint ToUInt() => To<uint>();
        public long ToLong() => To<long>();
        public ulong ToULong() => To<ulong>();
        public float ToFloat() => To<float>();
        public double ToDouble() => To<double>();
        public override string ToString() => (string) Value;

        public T To<T>()
        {
            T value;
            if (Value is string && typeof(T) != typeof(string))
            {
                try
                {
                    value = (T)Convert.ChangeType(Value, typeof(T));
                    return value;
                }
                catch
                {
                }
            }

            if (Value is T)
            {
                value = (T)Value;
                return value;
            }

            return default;
        }

        public static implicit operator bool(Global ret) => ret.To<bool>();
        public static implicit operator byte(Global ret) => ret.To<byte>();
        public static implicit operator sbyte(Global ret) => ret.To<sbyte>();
        public static implicit operator short(Global ret) => ret.To<short>();
        public static implicit operator ushort(Global ret) => ret.To<ushort>();
        public static implicit operator int(Global ret) => ret.To<int>();
        public static implicit operator uint(Global ret) => ret.To<uint>();
        public static implicit operator long(Global ret) => ret.To<long>();
        public static implicit operator ulong(Global ret) => ret.To<ulong>();
        public static implicit operator float(Global ret) => ret.To<float>();
        public static implicit operator double(Global ret) => ret.To<double>();
        public static implicit operator string(Global ret) => ret.To<string>();
    }
}