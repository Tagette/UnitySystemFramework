using UnitySystemFramework.Core;
using System.Collections.Generic;
using System.Linq;

namespace UnitySystemFramework.Identity
{
    public class IDSystem : BaseSystem
    {
        private readonly HashSet<ID> _mainLookup = new HashSet<ID>();
        private readonly Dictionary<ID, List<object>> _valuesByID = new Dictionary<ID, List<object>>();
        private readonly Dictionary<object, ID> _idsByValue = new Dictionary<object, ID>();

        private ulong _nextID;

        protected override void OnInit()
        {
            _nextID = ID.SESSION_MIN;
            // TODO: Load from file or override from a save.
            // TODO: Load all the static IDs.
        }

        protected override void OnStart()
        {
        }

        protected override void OnEnd()
        {
        }

        /// <summary>
        /// Determines if the ID give exists in the IDSystem.
        /// </summary>
        public bool Exists(ID id)
        {
            return _mainLookup.Contains(id);
        }

        /// <summary>
        /// Creates an ID using the next ID available. The entity may be set here.
        /// </summary>
        public ID Create()
        {
            var id = ID.FromRawID(_nextID++);
            Add(id);
            return id;
        }

        /// <inheritdoc cref="Create(Entity)" />
        public ID<T> Create<T>()
        {
            var id = ID<T>.FromRawID(_nextID++);
            Add(id);
            return id;
        }

        /// <summary>
        /// Adds an existing ID to the system. Throws a IDAlreadyExistsException if the provided ID already exists.
        /// </summary>
        public void Add(ID id)
        {
            if (!_mainLookup.Add(id))
                throw new IDAlreadyExistsException(id);
        }

        /// <summary>
        /// Removed an ID from the system.
        /// </summary>
        public void Remove(ID id)
        {
            UnregisterAll(id);
            _mainLookup.Remove(id);
        }

        /// <summary>
        /// Determines if the provided type is registered with the specified ID.
        /// </summary>
        public bool Has<T>(ID id)
        {
            if(!_valuesByID.TryGetValue(id, out var values))
            {
                return false;
            }

            return values.Any(v => v is T);
        }

        /// <summary>
        /// Registers a value with the specified ID. This value can be retrieved at the cost of a dictionary lookup 
        /// using <see cref="Get{T}(ID)"/>. Multiple objects of the same type are currently not supported.
        /// </summary>
        public void Register<T>(ID id, T value)
        {
            if (value == null)
                return;

            if (!_valuesByID.TryGetValue(id, out var values))
                _valuesByID[id] = values = new List<object>();

            if (!values.Contains(value))
                values.Add(value);

            if(!_idsByValue.ContainsKey(value))
                _idsByValue[value] = id;
        }

        /// <summary>
        /// Unregisters a value from the specified id.
        /// </summary>
        public void Unregister<T>(ID id, T value)
        {
            if (!_valuesByID.TryGetValue(id, out var values))
                return;

            _idsByValue.Remove(value);
            values.Remove(value);
        }

        /// <summary>
        /// Unregisters all values from the specified id.
        /// </summary>
        public void UnregisterAll(ID id)
        {
            if (!_valuesByID.TryGetValue(id, out var values))
                return;

            _valuesByID.Remove(id);

            foreach (var value in values)
            {
                if(_idsByValue.TryGetValue(value, out var otherID) && id == otherID)
                {
                    _idsByValue.Remove(value);
                }
            }
        }

        /// <summary>
        /// Retrieves the first ID using the provided object and unregisters the value from it. Currently this will 
        /// not update the lookup to replace with the next ID using the value.
        /// </summary>
        public void UnregisterFirst<T>(T value)
        {
            if (!_idsByValue.TryGetValue(value, out var id))
            {
                return;
            }

            if (_valuesByID.TryGetValue(id, out var values))
            {
                if (values.Remove(value))
                {
                    _idsByValue.Remove(value);
                }
            }
        }

        /// <summary>
        /// Gets the first value that matches the specified type and is registered to the specified id.
        /// </summary>
        public T Get<T>(ID id)
        {
            if (!_valuesByID.TryGetValue(id, out var values))
                return default;

            foreach(var value in values)
            {
                if(value is T)
                {
                    return (T)value;
                }
            }

            return default;
        }

        /// <summary>
        /// Retrieves the ID that was used to register the value specified.
        /// </summary>
        public ID GetID<T>(T value)
        {
            _idsByValue.TryGetValue(value, out var id);
            return id;
        }
    }
}
