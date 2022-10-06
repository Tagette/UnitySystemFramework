using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Unity.Collections.LowLevel.Unsafe;

namespace UnitySystemFramework
{
    public static class Reflect
    {
        private abstract class TypeChecker
        {
            public abstract bool Is<T>(T value);
        }

        private class TypeChecker<T> : TypeChecker
        {
            public override bool Is<T1>(T1 value)
            {
                return value is T;
            }
        }

        private static readonly Type _typeCheckGeneric = typeof(TypeChecker<>);

        private static readonly List<Assembly> _assemblies = new List<Assembly>();
        private static readonly List<Type> _types = new List<Type>();

        private static readonly Dictionary<Type, TypeID> _typeForCode = new Dictionary<Type, TypeID>();
        private static readonly Dictionary<TypeID, Type> _codeForType = new Dictionary<TypeID, Type>();
        private static readonly Dictionary<TypeID, int> _typeSizes = new Dictionary<TypeID, int>();
        private static readonly Dictionary<TypeID, string> _typeNames = new Dictionary<TypeID, string>();
        private static readonly Dictionary<TypeID, IList<TypeID>> _interfaces = new Dictionary<TypeID, IList<TypeID>>();
        private static readonly Dictionary<TypeID, IList<TypeID>> _implementors = new Dictionary<TypeID, IList<TypeID>>();
        private static readonly Dictionary<TypeID, ConstructorInfo> _constructors = new Dictionary<TypeID, ConstructorInfo>();
        private static readonly Dictionary<TypeID, IList<MethodInfo>> _typeMethods = new Dictionary<TypeID, IList<MethodInfo>>();
        private static readonly Dictionary<MethodInfo, List<Attribute>> _methodAttributes = new Dictionary<MethodInfo, List<Attribute>>();
        private static readonly Dictionary<TypeID, IList<MethodInfo>> _attributeMethods = new Dictionary<TypeID, IList<MethodInfo>>();
        private static readonly Dictionary<TypeID, TypeChecker> _typeCheckers = new Dictionary<TypeID, TypeChecker>();

        public static IList<TypeID> AllTypes { get; } = new List<TypeID>();

        public static IEnumerable<Assembly> GetUserAssemblies()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var systemAssembly = typeof(object).Assembly;
            var unityAssembly = typeof(UnityEngine.Object).Assembly;

            for (int i = 0; i < allAssemblies.Length; i++)
            {
                var assembly = allAssemblies[i];
                if (assembly == systemAssembly || assembly == unityAssembly || assembly.FullName.StartsWith("Unity") || assembly.FullName.StartsWith("System"))
                    continue;

                yield return assembly;
            }
        }

        public static IEnumerable<Assembly> GetUnityAssemblies()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var systemAssembly = typeof(object).Assembly;
            var unityAssembly = typeof(UnityEngine.Object).Assembly;

            for (int i = 0; i < allAssemblies.Length; i++)
            {
                var assembly = allAssemblies[i];
                if (assembly == systemAssembly || assembly.FullName.StartsWith("System"))
                    continue;

                if (assembly != unityAssembly && !assembly.FullName.StartsWith("Unity"))
                    continue;

                yield return assembly;
            }
        }

        public static void CacheCurrentAssembly()
        {
            var assembly = Assembly.GetCallingAssembly();
            CacheAssembly(assembly);
        }

        public static void CacheAssemblies(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                CacheAssembly(assembly);
            }
        }

        public static void CacheAssembly(Assembly assembly)
        {
            if (_assemblies.Contains(assembly))
                return;

            _assemblies.Add(assembly);

            var types = assembly.GetTypes();
            _types.AddRange(types);

            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                CacheType(type);
            }
        }

        public static TypeID CacheType(Type type)
        {
            if(type == typeof(void))
                return TypeID.NoType;

            if (_typeForCode.TryGetValue(type, out var typeID))
                return typeID;

            var typeName = type.Name;
            if(typeName.StartsWith("<>") || typeName.StartsWith("{"))
                return TypeID.NoType;

            typeID = new TypeID(type.AssemblyQualifiedName.GetHashCode());

            if (_codeForType.TryGetValue(typeID, out var conflict))
                throw new InvalidOperationException($"Two types have the same typeID ({typeID._ID}) and are colliding. ({typeID.Name}, {conflict.Name})");

            AllTypes.Add(typeID);

            _typeForCode.Add(type, typeID);
            _codeForType.Add(typeID, type);
            _typeNames.Add(typeID, type.Name);
            if (type.IsValueType)
                _typeSizes.Add(typeID, UnsafeUtility.SizeOf(type));

            var methods = new List<MethodInfo>(type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
            _typeMethods[typeID] = methods.AsReadOnly();
            
            var genericType = _typeCheckGeneric.MakeGenericType(type);
            _typeCheckers[typeID] = (TypeChecker) FormatterServices.GetUninitializedObject(genericType);

            return typeID;
        }

        public static IList<MethodInfo> GetMethods(TypeID typeID)
        {
            if (typeID.IsNoType)
                return Array.Empty<MethodInfo>();

            if (_typeMethods.TryGetValue(typeID, out var cache))
                return cache;

            var methods = new List<MethodInfo>(typeID.Type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static));
            _typeMethods[typeID] = cache = methods.AsReadOnly();
            return cache;
        }

        public static IList<TypeID> GetInterfaces(TypeID typeID)
        {
            if (typeID.IsNoType)
                return Array.Empty<TypeID>();

            if (_interfaces.TryGetValue(typeID, out var cache))
                return cache;

            var interfaceIDs = new List<TypeID>();
            var interfaces = typeID.Type.GetInterfaces();
            for (int i = 0; i < interfaces.Length; i++)
            {
                interfaceIDs.Add(interfaces[i].GetTypeID());
            }
            _interfaces[typeID] = cache = interfaceIDs.AsReadOnly();

            return cache;
        }

        public static IList<TypeID> GetImplementors(Type type)
        {
            return GetImplementors(type.GetTypeID());
        }

        public static IList<TypeID> GetImplementors(TypeID typeID)
        {
            if (typeID.IsNoType)
                return Array.Empty<TypeID>();

            if (_implementors.TryGetValue(typeID, out IList<TypeID> cache))
                return cache;

            var types = new List<TypeID>();

            for (int i = 0; i < _types.Count; i++)
            {
                var t = _types[i];
                if (typeID.Type.IsAssignableFrom(t))
                {
                    types.Add(t.GetTypeID());
                }
            }

            _implementors.Add(typeID, cache = types.AsReadOnly());

            return cache;
        }

        public static bool TryGetType(TypeID typeID, out Type type)
        {
            type = default;
            if (typeID.IsNoType)
                return false;
            return _codeForType.TryGetValue(typeID, out type);
        }

        public static Type GetTypeFromID(TypeID typeID)
        {
            if (typeID.IsNoType)
                return default;
            _codeForType.TryGetValue(typeID, out Type type);
            return type;
        }

        public static bool TryGetID(Type type, out TypeID typeID)
        {
            return _typeForCode.TryGetValue(type, out typeID);
        }

        public static TypeID GetTypeID(this Type type)
        {
            _typeForCode.TryGetValue(type, out TypeID typeID);
            return typeID;
        }

        public static TypeID GetTypeID<T>(this T value)
        {
            if(value == null)
                return TypeID.NoType;

            var type = value.GetType();
            _typeForCode.TryGetValue(type, out TypeID typeID);
            return typeID;
        }

        public static int GetTypeSize(TypeID typeID)
        {
            if (typeID.IsNoType)
                return 0;
            _typeSizes.TryGetValue(typeID, out int size);
            return size;
        }

        public static string GetTypeName(TypeID typeID)
        {
            if (typeID.IsNoType)
                return default;
            _typeNames.TryGetValue(typeID, out string name);
            return name;
        }

        public static int GetTypeSize(Type type)
        {
            if (!_typeSizes.TryGetValue(type.GetTypeID(), out int size))
            {
                var typeID = CacheType(type);
                _typeSizes.TryGetValue(typeID, out size);
            }
            return size;
        }

        public static bool IsType<T>(T value, TypeID typeID)
        {
            if (_typeCheckers.TryGetValue(typeID, out var typeChecker))
                return typeChecker.Is(value);

            return TypeID<T>.Type.IsSubclassOf(typeID.Type);
        }

        public static bool Instantiate(Type type, out object value)
        {
            value = null;
            return _typeForCode.TryGetValue(type, out TypeID typeID) && Instantiate(typeID, out value);
        }

        public static bool Instantiate(TypeID typeID, out object value)
        {
            value = default;
            if (typeID.IsNoType)
                return false;

            if (!_constructors.TryGetValue(typeID, out ConstructorInfo constructor))
                _constructors[typeID] = constructor = typeID.Type.GetConstructor(Type.EmptyTypes);

            if (constructor == null)
                return false;

            value = constructor.Invoke(null);
            return true;
        }

        public static bool Instantiate<T>(Type type, out T value)
        {
            value = default;
            return _typeForCode.TryGetValue(type, out TypeID typeID) && Instantiate(typeID, out value);
        }

        public static bool Instantiate<T>(TypeID typeID, out T value)
        {
            value = default;
            if (typeID.IsNoType)
                return false;

            if (!_constructors.TryGetValue(typeID, out ConstructorInfo constructor))
                _constructors[typeID] = constructor = typeID.Type.GetConstructor(Type.EmptyTypes);

            if (constructor == null)
                return false;

            try
            {
                value = (T)constructor.Invoke(null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsStatic(this MemberInfo member)
        {
            if (member is MethodInfo method)
                return method.IsStatic;

            if (member is PropertyInfo property)
            {
                var getter = property.GetGetMethod();
                var setter = property.GetSetMethod();

                return (getter != null && getter.IsStatic) || (setter != null && setter.IsStatic);
            }

            if (member is FieldInfo field)
                return field.IsStatic;

            if (member is TypeInfo type)
                return type.IsAbstract && type.IsSealed;

            if (member is ConstructorInfo constructor)
                return constructor.IsStatic;

            if (member is EventInfo evt)
            {
                var adder = evt.GetAddMethod();
                var remover = evt.GetRemoveMethod();

                return (adder != null && adder.IsStatic) || (remover != null && remover.IsStatic);
            }

            return false;
        }

        public static IList<MethodInfo> GetMethodsWithAttribute<T>()
        {
            return GetMethodsWithAttribute(TypeID<T>.ID);
        }

        public static IList<MethodInfo> GetMethodsWithAttribute(TypeID attributeType)
        {
            if (attributeType.IsNoType)
                return Array.Empty<MethodInfo>();

            if (_attributeMethods.TryGetValue(attributeType, out var cache))
                return cache;

            var attributeMethods = new List<MethodInfo>();
            foreach (var type in AllTypes)
            {
                if (_typeMethods.TryGetValue(type, out var methods))
                {
                    foreach (var method in methods)
                    {
                        try
                        {
                            if (!_methodAttributes.TryGetValue(method, out var attributes))
                                _methodAttributes[method] = attributes = method.GetCustomAttributes().ToList();

                            if (attributes.Any(a => attributeType.Type.IsInstanceOfType(a)))
                                attributeMethods.Add(method);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            _attributeMethods[attributeType] = cache = attributeMethods.AsReadOnly();
            return cache;
        }
    }
}
