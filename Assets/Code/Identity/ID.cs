using UnitySystemFramework.Core;
using System;
using UnityEngine;

namespace UnitySystemFramework.Identity
{
    public struct ID : IEquatable<ID>
    {
        public const ulong SESSION_MIN = 1_000_000;
        public const ulong SESSION_MAX = ulong.MaxValue;

        public const ulong STATIC_MIN = 0;
        public const ulong STATIC_MAX = SESSION_MIN - 1;

        /// <summary>
        /// The default, unassigned value of an ID.
        /// </summary>
        public static ID Default = default;

        /// <summary>
        /// Converts a raw id into an ID.
        /// </summary>
        public static ID FromRawID(ulong id)
        {
            return new ID()
            {
                _id = id,
            };
        }

        [SerializeField]
        private ulong _id;

        /// <summary>
        /// Determines whether or not the current ID is the default value. (aka unassigned)
        /// </summary>
        public bool IsValid
        {
            get
            {
                return _id != 0;
            }
        }

        /// <summary>
        /// Determines whether or not this is a static ID.
        /// </summary>
        public bool IsStatic => _id <= STATIC_MAX;

        /// <summary>
        /// The inner ID of the id. This includes an entity if one exists.
        /// </summary>
        public ulong RawID
        {
            get => _id;
            set => _id = value;
        }

        /// <summary>
        /// Converts a non-typed ID into a typed ID.
        /// </summary>
        public ID<T> To<T>()
        {
            return (ID<T>)this;
        }

        /// <inheritdoc cref="IDSystem.Get{T}(ID)" />
        public T Get<T>()
        {
            var game = Game.CurrentGame;
            var idSystem = game.GetSystem<IDSystem>();
            return idSystem.Get<T>(this);
        }

        /// <inheritdoc cref="IDSystem.Get{T}(ID)" />
        public T Get<T>(ID<T> id)
        {
            var game = Game.CurrentGame;
            var idSystem = game.GetSystem<IDSystem>();
            return idSystem.Get<T>(id);
        }

        /// <inheritdoc cref="IDSystem.Has{T}(ID)" />
        public bool Has<T>()
        {
            var game = Game.CurrentGame;
            var idSystem = game.GetSystem<IDSystem>();
            return idSystem.Has<T>(this);
        }

        /// <inheritdoc cref="IDSystem.Register{T}(ID, T)" />
        public ID Register<T>(T value)
        {
            var game = Game.CurrentGame;
            var idSystem = game.GetSystem<IDSystem>();
            idSystem.Register(this, value);

            return this;
        }

        /// <inheritdoc cref="IDSystem.Unregister{T}(ID, T)" />
        public ID Unregister<T>(T value)
        {
            var game = Game.CurrentGame;
            var idSystem = game.GetSystem<IDSystem>();
            idSystem.Unregister(this, value);

            return this;
        }

        /// <inheritdoc cref="IDSystem.UnregisterAll(ID)" />
        public ID UnregisterAll()
        {
            var game = Game.CurrentGame;
            var idSystem = game.GetSystem<IDSystem>();
            idSystem.UnregisterAll(this);

            return this;
        }

        public override string ToString()
        {
            if (IsStatic)
            {
                return $"<{_id}>";
            }
            else
            {
                return $"[{_id}]";
            }
        }

        #region IEquatable<ID>

        public bool Equals(ID other)
        {
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            return obj is ID other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        #endregion // IEquatable<ID>

        public static bool operator ==(ID a, ID b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ID a, ID b)
        {
            return !a.Equals(b);
        }
    }

    public struct ID<T>
    {
        /// <inheritdoc cref="ID.Default" />
        public static ID<T> Default = default;

        /// <inheritdoc cref="ID.FromRawID(ulong)" />
        public static ID<T> FromRawID(ulong id)
        {
            return new ID<T>()
            {
                _id = id,
            };
        }

        private ulong _id;

        /// <inheritdoc cref="ID.IsValid" />
        public bool IsValid => ToID().IsValid;

        /// <inheritdoc cref="ID.IsStatic" />
        public bool IsStatic => ToID().IsStatic;

        /// <inheritdoc cref="ID.RawID" />
        public ulong RawID
        {
            get => _id;
            set => _id = value;
        }

        /// <summary>
        /// Converts this to an ID without a type.
        /// </summary>
        public ID ToID()
        {
            return new ID()
            {
                RawID = _id,
            };
        }

        /// <summary>
        /// Converts a non-typed ID into a typed ID.
        /// </summary>
        public ID<T2> To<T2>()
        {
            return new ID<T2>()
            {
                _id = this._id,
            };
        }

        /// <inheritdoc cref="IDSystem.Get{T}(ID)" />
        public T2 Get<T2>()
        {
            var game = Game.CurrentGame;
            var idSystem = game.GetSystem<IDSystem>();
            return idSystem.Get<T2>(this);
        }

        /// <inheritdoc cref="IDSystem.Has{T}(ID)" />
        public bool Has<T2>()
        {
            var game = Game.CurrentGame;
            var idSystem = game.GetSystem<IDSystem>();
            return idSystem.Has<T2>(this);
        }

        /// <inheritdoc cref="IDSystem.Register{T}(ID, T)" />
        public ID<T> Register<T2>(T2 value)
        {
            var game = Game.CurrentGame;
            var idSystem = game.GetSystem<IDSystem>();
            idSystem.Register(this, value);

            return this;
        }

        /// <inheritdoc cref="IDSystem.Unregister{T}(ID, T)" />
        public ID<T> Unregister<T2>(T2 value)
        {
            var game = Game.CurrentGame;
            var idSystem = game.GetSystem<IDSystem>();
            idSystem.Unregister(this, value);

            return this;
        }

        /// <inheritdoc cref="IDSystem.UnregisterAll(ID)" />
        public ID<T> UnregisterAll()
        {
            var game = Game.CurrentGame;
            var idSystem = game.GetSystem<IDSystem>();
            idSystem.UnregisterAll(this);

            return this;
        }

        public override string ToString()
        {
            return $"{TypeID<T>.Name}{ToID()}";
        }

        #region IEquatable<ID>

        public bool Equals(ID<T> other)
        {
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            return obj is ID<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        #endregion // IEquatable<ID>

        public static bool operator ==(ID<T> a, ID<T> b)
        {
            return a._id == b._id;
        }

        public static bool operator !=(ID<T> a, ID<T> b)
        {
            return a._id != b._id;
        }

        public static bool operator ==(ID<T> a, ID b)
        {
            return a._id == b.RawID;
        }

        public static bool operator !=(ID<T> a, ID b)
        {
            return a._id != b.RawID;
        }

        public static bool operator ==(ID a, ID<T> b)
        {
            return a.RawID == b._id;
        }

        public static bool operator !=(ID a, ID<T> b)
        {
            return a.RawID != b._id;
        }

        public static implicit operator ID(ID<T> id)
        {
            return new ID()
            {
                RawID = id.RawID,
            };
        }

        public static explicit operator ID<T>(ID id)
        {
            return new ID<T>()
            {
                RawID = id.RawID,
            };
        }
    }
}
