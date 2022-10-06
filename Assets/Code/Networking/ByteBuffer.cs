using System;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;

namespace UnitySystemFramework.Serialization
{
    public unsafe class ByteBuffer
    {
        /// <summary>
        /// The internal byte array.
        /// </summary>
        public byte[] _Data;
        
        /// <summary>
        /// The length of the data in bytes.
        /// </summary>
        public int _Length;

        /// <summary>
        /// The current reading position.
        /// </summary>
        public int _Position;

        /// <summary>
        /// The reference count for this buffer.
        /// </summary>
        public int _RefCount;

        private byte* _Pointer
        {
            get
            {
                fixed (byte* b = _Data) return b;
            }
        }

        /// <summary>
        /// Set this property to true to prevent this buffer from being released. 
        /// Some release functions in other systems may override this behaviour.
        /// </summary>
        public bool DoNotRelease { get; set; }

        public ByteBuffer(byte[] data)
        {
            _Data = data;
            _Length = data.Length;
            _Position = 0;
        }

        public ByteBuffer(byte[] data, int offset)
        {
            _Data = data;
            _Length = data.Length;
            _Position = offset;
        }

        public ByteBuffer(byte[] data, int offset, int length)
        {
            _Data = data;
            _Length = length;
            _Position = offset;
        }

        public int Capacity => _Data.Length;

        public int RemainingBytes => _Length - _Position;

        public byte this[int index]
        {
            get => *(_Pointer + index);
            set => *(_Pointer + index) = value;
        }

        public void EnsureCapacity(int capacity)
        {
            if (Capacity >= capacity)
                return;

            int newCapacity = Capacity;
            while (newCapacity < capacity)
                newCapacity *= 2;

            var old = _Data;
            _Data = new byte[newCapacity];
            Buffer.BlockCopy(old, 0, _Data, 0, _Length);
        }

        public void Reset()
        {
            _Position = 0;
            _Length = 0;
        }

        public void Skip(int byteAmount)
        {
            _Position += byteAmount;
        }

        public void SkipBool()
        {
            _Position += sizeof(bool);
        }

        public void SkipByte()
        {
            _Position += sizeof(byte);
        }

        public void SkipSByte()
        {
            _Position += sizeof(sbyte);
        }

        public void SkipChar()
        {
            _Position += sizeof(char);
        }

        public void SkipShort()
        {
            _Position += sizeof(short);
        }

        public void SkipUShort()
        {
            _Position += sizeof(ushort);
        }

        public void SkipInt()
        {
            _Position += sizeof(int);
        }

        public void SkipUInt()
        {
            _Position += sizeof(uint);
        }

        public void SkipFloat()
        {
            _Position += sizeof(float);
        }

        public void SkipDouble()
        {
            _Position += sizeof(double);
        }

        public void SkipLong()
        {
            _Position += sizeof(long);
        }

        public void SkipULong()
        {
            _Position += sizeof(ulong);
        }

        public void SkipBytes()
        {
            _Position += ReadInt();
        }

        public void SkipString()
        {
            int length = ReadInt();
            if (length == 0)
                return;

            _Position += length;
        }

        public void SkipUnmanaged<T>() where T : unmanaged
        {
            _Position += sizeof(T);
        }

        public void SkipStruct<T>() where T : struct
        {
            int size = UnsafeUtility.SizeOf<T>();
            _Position += size;
        }

        public void SkipBuffer()
        {
            var length = ReadInt();
            _Position += length;
        }

        public bool ReadBool()
        {
            var value = *(bool*)(_Pointer + _Position);
            _Position += sizeof(bool);
            return value;
        }

        public byte ReadByte()
        {
            var value = *(byte*)(_Pointer + _Position);
            _Position += sizeof(byte);
            return value;
        }

        public sbyte ReadSByte()
        {
            var value = *(sbyte*)(_Pointer + _Position);
            _Position += sizeof(sbyte);
            return value;
        }

        public char ReadChar()
        {
            var value = *(char*)(_Pointer + _Position);
            _Position += sizeof(char);
            return value;
        }

        public short ReadShort()
        {
            var value = *(short*)(_Pointer + _Position);
            _Position += sizeof(short);
            return value;
        }

        public ushort ReadUShort()
        {
            var value = *(ushort*)(_Pointer + _Position);
            _Position += sizeof(ushort);
            return value;
        }

        public int ReadInt()
        {
            var value = *(int*)(_Pointer + _Position);
            _Position += sizeof(int);
            return value;
        }

        public uint ReadUInt()
        {
            var value = *(uint*)(_Pointer + _Position);
            _Position += sizeof(uint);
            return value;
        }

        public float ReadFloat()
        {
            var value = *(float*)(_Pointer + _Position);
            _Position += sizeof(float);
            return value;
        }

        public double ReadDouble()
        {
            var value = *(double*)(_Pointer + _Position);
            _Position += sizeof(double);
            return value;
        }

        public long ReadLong()
        {
            var value = *(long*)(_Pointer + _Position);
            _Position += sizeof(long);
            return value;
        }

        public ulong ReadULong()
        {
            var value = *(ulong*)(_Pointer + _Position);
            _Position += sizeof(ulong);
            return value;
        }

        public int ReadBytes(byte[] bytes, int offset)
        {
            fixed (byte* b = bytes)
            {
                int length = ReadInt();
                Buffer.MemoryCopy(_Pointer + _Position, b + offset, bytes.Length - offset, length);
                _Position += length;
                return length;
            }
        }

        public string ReadString(Encoding encoding = null)
        {
            int length = ReadInt();
            if (length == 0)
                return null;
            if (encoding == null)
                encoding = Encoding.UTF8;
            var value = encoding.GetString(_Pointer + _Position, length);
            _Position += length;
            return value;
        }

        public T ReadUnmanaged<T>() where T : unmanaged
        {
            if (_Position + sizeof(T) > _Length)
                throw new IndexOutOfRangeException("Not enough remaining bytes.");
            var value = *(T*)(_Pointer + _Position);
            _Position += sizeof(T);
            return value;
        }

        public T ReadStruct<T>() where T : struct
        {
            int size = UnsafeUtility.SizeOf<T>();
            if (_Position + size > _Length)
                throw new IndexOutOfRangeException("Not enough remaining bytes.");
            var ptr = _Pointer + _Position;
            UnsafeUtility.CopyPtrToStructure(ptr, out T val);
            _Position += size;

            return val;
        }

        public void ReadBuffer(ByteBuffer buffer)
        {
            var length = ReadInt();
            buffer.WriteBytes(_Data, _Position, length);
            _Position += length;
        }

        public bool ReadBoolAt(int position)
        {
            return *(bool*)(_Pointer + position);
        }

        public byte ReadByteAt(int position)
        {
            return *(byte*)(_Pointer + _Position);
        }

        public sbyte ReadSByteAt(int position)
        {
            return *(sbyte*)(_Pointer + _Position);
        }

        public char ReadCharAt(int position)
        {
            return *(char*)(_Pointer + _Position);
        }

        public short ReadShortAt(int position)
        {
            return *(short*)(_Pointer + _Position);
        }

        public ushort ReadUShortAt(int position)
        {
            return *(ushort*)(_Pointer + _Position);
        }

        public int ReadIntAt(int position)
        {
            return *(int*)(_Pointer + _Position);
        }

        public uint ReadUIntAt(int position)
        {
            return *(uint*)(_Pointer + _Position);
        }

        public float ReadFloatAt(int position)
        {
            return *(float*)(_Pointer + _Position);
        }

        public double ReadDoubleAt(int position)
        {
            return *(double*)(_Pointer + _Position);
        }

        public long ReadLongAt(int position)
        {
            return *(long*)(_Pointer + _Position);
        }

        public ulong ReadULongAt(int position)
        {
            return *(ulong*)(_Pointer + _Position);
        }

        public string ReadStringAt(int position, Encoding encoding = null)
        {
            int length = ReadIntAt(position);
            if (length == 0)
                return null;
            if (encoding == null)
                encoding = Encoding.UTF8;
            return encoding.GetString(_Pointer + position + sizeof(int), length);
        }

        public T ReadUnmanagedAt<T>(int position) where T : unmanaged
        {
            if (position + sizeof(T) > _Length)
                throw new IndexOutOfRangeException("Not enough remaining bytes.");
            return *(T*)(_Pointer + position);
        }

        public T ReadStructAt<T>(int position) where T : struct
        {
            int size = UnsafeUtility.SizeOf<T>();
            if (position + size > _Length)
                throw new IndexOutOfRangeException("Not enough remaining bytes.");
            var ptr = _Pointer + position;
            UnsafeUtility.CopyPtrToStructure(ptr, out T val);

            return val;
        }

        public void ReadBufferAt(int position, ByteBuffer byteBuffer)
        {
            var length = ReadIntAt(position);
            byteBuffer.WriteBytes(_Data, position + length, length);
        }

        public void WriteBool(bool value)
        {
            EnsureCapacity(_Length + sizeof(bool));
            *(bool*)(_Pointer + _Length) = value;
            _Length += sizeof(bool);
        }

        public void WriteByte(byte value)
        {
            EnsureCapacity(_Length + sizeof(byte));
            *(byte*)(_Pointer + _Length) = value;
            _Length += sizeof(byte);
        }

        public void WriteSByte(sbyte value)
        {
            EnsureCapacity(_Length + sizeof(sbyte));
            *(sbyte*)(_Pointer + _Length) = value;
            _Length += sizeof(sbyte);
        }

        public void WriteChar(char value)
        {
            EnsureCapacity(_Length + sizeof(char));
            *(char*)(_Pointer + _Length) = value;
            _Length += sizeof(char);
        }

        public void WriteShort(short value)
        {
            EnsureCapacity(_Length + sizeof(short));
            *(short*)(_Pointer + _Length) = value;
            _Length += sizeof(short);
        }

        public void WriteUShort(ushort value)
        {
            EnsureCapacity(_Length + sizeof(ushort));
            *(ushort*)(_Pointer + _Length) = value;
            _Length += sizeof(ushort);
        }

        public void WriteInt(int value)
        {
            EnsureCapacity(_Length + sizeof(int));
            *(int*)(_Pointer + _Length) = value;
            _Length += sizeof(int);
        }

        public void WriteUInt(uint value)
        {
            EnsureCapacity(_Length + sizeof(uint));
            *(uint*)(_Pointer + _Length) = value;
            _Length += sizeof(uint);
        }

        public void WriteFloat(float value)
        {
            EnsureCapacity(_Length + sizeof(float));
            *(float*)(_Pointer + _Length) = value;
            _Length += sizeof(float);
        }

        public void WriteDouble(double value)
        {
            EnsureCapacity(_Length + sizeof(double));
            *(double*)(_Pointer + _Length) = value;
            _Length += sizeof(double);
        }

        public void WriteLong(long value)
        {
            EnsureCapacity(_Length + sizeof(long));
            *(long*)(_Pointer + _Length) = value;
            _Length += sizeof(long);
        }

        public void WriteULong(ulong value)
        {
            EnsureCapacity(_Length + sizeof(ulong));
            *(ulong*)(_Pointer + _Length) = value;
            _Length += sizeof(ulong);
        }

        public void WriteBytes(byte[] bytes, int offset, int count)
        {
            fixed (byte* b = bytes)
            {
                WriteInt(count);
                EnsureCapacity(_Length + count);
                Buffer.MemoryCopy(b + offset, _Pointer + _Length, _Length, count);
                _Length += count;
            }
        }

        public void WriteString(string value, Encoding encoding = null)
        {
            if (value == null)
            {
                WriteInt(0);
                return;
            }
            if (encoding == null)
                encoding = Encoding.UTF8;
            int length = encoding.GetByteCount(value);
            WriteInt(length);

            EnsureCapacity(_Length + length);
            fixed (char* c = value)
                encoding.GetBytes(c, value.Length, _Pointer + _Length, Capacity - _Length);
            _Length += length;
        }

        public T WriteUnmanaged<T>(T value) where T : unmanaged
        {
            EnsureCapacity(_Length + sizeof(T));
            *(T*)(_Pointer + _Length) = value;
            _Length += sizeof(T);

            return value;
        }

        public T WriteStruct<T>(T value) where T : struct
        {
            int size = UnsafeUtility.SizeOf<T>();
            EnsureCapacity(_Length + size);
            var ptr = _Pointer + _Length;
            UnsafeUtility.CopyStructureToPtr(ref value, ptr);
            _Length += size;

            return value;
        }

        public void WriteBuffer(ByteBuffer byteBuffer)
        {
            WriteInt(byteBuffer._Length);
            WriteBytes(byteBuffer._Data, 0, byteBuffer._Length);
        }

        public void WriteBoolAt(int position, bool value)
        {
            *(bool*)(_Pointer + position) = value;
        }

        public void WriteByteAt(int position, byte value)
        {
            *(byte*)(_Pointer + position) = value;
        }

        public void WriteSByteAt(int position, sbyte value)
        {
            *(sbyte*)(_Pointer + position) = value;
        }

        public void WriteCharAt(int position, char value)
        {
            *(char*)(_Pointer + position) = value;
        }

        public void WriteShortAt(int position, short value)
        {
            *(short*)(_Pointer + position) = value;
        }

        public void WriteUShortAt(int position, ushort value)
        {
            *(ushort*)(_Pointer + position) = value;
        }

        public void WriteIntAt(int position, int value)
        {
            *(int*)(_Pointer + position) = value;
        }

        public void WriteUIntAt(int position, uint value)
        {
            *(uint*)(_Pointer + position) = value;
        }

        public void WriteFloatAt(int position, float value)
        {
            *(float*)(_Pointer + position) = value;
        }

        public void WriteDoubleAt(int position, double value)
        {
            *(double*)(_Pointer + position) = value;
        }

        public void WriteLongAt(int position, long value)
        {
            *(long*)(_Pointer + position) = value;
        }

        public void WriteULongAt(int position, ulong value)
        {
            *(ulong*)(_Pointer + position) = value;
        }

        public void WriteBytesAt(int position, byte[] bytes, int offset, int count)
        {
            fixed (byte* b = bytes)
            {
                WriteIntAt(position, count);
                Buffer.MemoryCopy(b + offset, _Pointer + position, count, count);
            }
        }

        public void WriteStringAt(int position, string value, Encoding encoding = null)
        {
            int prevLength = ReadIntAt(position);

            if (value == null)
            {
                WriteIntAt(position, -1);
                return;
            }

            if (value.Length == 0)
            {
                WriteIntAt(position, 0);
                return;
            }

            if(encoding == null)
                encoding = Encoding.UTF8;

            int length = encoding.GetByteCount(value);

            if (length != prevLength)
                throw new InvalidOperationException("You cannot write a string that is a different length then the previous one.");

            WriteIntAt(position, length);
            fixed (char* c = value)
                encoding.GetBytes(c, value.Length, _Pointer + position, length);
        }

        public T WriteUnmanagedAt<T>(int position, T value) where T : unmanaged
        {
            if (position + sizeof(T) > Capacity)
                throw new IndexOutOfRangeException("Not enough capacity left.");
            *(T*)(_Pointer + position) = value;

            return value;
        }

        public T WriteStructAt<T>(int position, T value) where T : struct
        {
            int size = UnsafeUtility.SizeOf<T>();
            if (position + size > Capacity)
                throw new IndexOutOfRangeException("Not enough capacity left.");
            var ptr = _Pointer + position;
            UnsafeUtility.CopyStructureToPtr(ref value, ptr);

            return value;
        }

        public void WriteBufferAt(int position, ByteBuffer byteBuffer)
        {
            int prevLength = ReadIntAt(position);

            if (byteBuffer._Length != prevLength)
                throw new InvalidOperationException("You cannot write a buffer that is a different length then the previous one.");

            WriteBytesAt(position + sizeof(int), byteBuffer._Data, 0, byteBuffer._Length);
        }
    }
}
