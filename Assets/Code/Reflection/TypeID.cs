using System;

namespace UnitySystemFramework
{
    [Serializable]
    public readonly struct TypeID : IEquatable<TypeID>
    {
        public static TypeID NoType = default;

        public bool IsNoType => this == NoType;

        public TypeID(Type type)
        {
            _ID = type.GetTypeID()._ID;
        }

        public TypeID(int id)
        {
            _ID = id;
        }

        public readonly int _ID;

        /// <summary>
        /// Gets the type from the <see cref="Reflect"/>.
        /// </summary>
        public Type Type => Reflect.GetTypeFromID(this);

        /// <summary>
        /// Gets the size of the type in bytes.
        /// </summary>
        public int Size => Reflect.GetTypeSize(this);

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        public string Name => Reflect.GetTypeName(this);

        /// <summary>
        /// Determines if the provided value is this type or a subclass of this type.
        /// </summary>
        public bool Is<T>(T value) => Reflect.IsType(value, this);

        /// <summary>
        /// Determines if the type <typeparamref name="T"/> is the same as this type.
        /// </summary>
        public bool Equals<T>()
        {
            return TypeID<T>.ID.Equals(_ID);
        }

        /// <summary>
        /// Determines if the type given is the same as this type.
        /// </summary>
        public bool Equals(Type type)
        {
            return type.GetTypeID()._ID == _ID;
        }

        /// <summary>
        /// Determines if this type id equals another type id.
        /// </summary>
        public bool Equals(TypeID other)
        {
            return _ID == other._ID;
        }

        /// <summary>
        /// Using this equals will box the type id.
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is TypeID other && Equals(other);
        }

        /// <summary>
        /// Returns the inner type id as a hashcode.
        /// </summary>
        public override int GetHashCode()
        {
            return _ID;
        }

        public static bool operator ==(TypeID a, TypeID b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TypeID a, TypeID b)
        {
            return !a.Equals(b);
        }

        public static implicit operator TypeID(Type type)
        {
            return new TypeID(type);
        }

        public static implicit operator Type(TypeID typeID)
        {
            return typeID.Type;
        }
    }

    public struct TypeID<T>
    {
        public TypeID Value;

        /// <summary>
        /// The system type of the type.
        /// </summary>
        public static readonly Type Type;

        /// <summary>
        /// The <see cref="TypeID"/> for this type.
        /// </summary>
        public static readonly TypeID ID;

        /// <summary>
        /// The unmanaged size of the type.
        /// </summary>
        public static readonly int Size;

        /// <summary>
        /// The name of the type.
        /// </summary>
        public static readonly string Name;

        static TypeID()
        {
            Type = typeof(T);
            ID = Type.GetTypeID();
            Size = Reflect.GetTypeSize(ID);
            Name = ID.Name ?? Type.Name;
        }

        public static implicit operator TypeID<T>(Type type)
        {
            return new TypeID<T>()
            {
                Value = new TypeID(type),
            };
        }

        public static implicit operator TypeID<T>(TypeID typeID)
        {
            return new TypeID<T>()
            {
                Value = typeID,
            };
        }

        public static implicit operator TypeID(TypeID<T> typeID)
        {
            return typeID.Value;
        }

        public static implicit operator Type(TypeID<T> typeID)
        {
            return typeID.Value.Type;
        }
    }
}
