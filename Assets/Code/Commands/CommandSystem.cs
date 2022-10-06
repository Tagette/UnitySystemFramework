using UnitySystemFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnitySystemFramework.Commands
{
    public delegate bool ReadParameterMethod(CommandParameter parameter, CommandReader reader, out object value);

    public struct CommandParameter
    {
        public Type Type;
        public string Name;
        public int WordLimit;
    }

    public class CommandSystem : BaseSystem
    {
        private class CommandInfo
        {
            public string Name;
            public MethodInfo Method;
            public object Target;
            public bool IsAlias;
            public string[] Aliases;
            public CommandParameter[] Parameters;
        }

        private class CommandParameterType
        {
            public Type Type;
            public ReadParameterMethod Reader;
            public string[] Syntax;
        }

        private readonly Dictionary<string, CommandInfo> _commands = new Dictionary<string, CommandInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<Type, CommandParameterType> _typeReaders = new Dictionary<Type, CommandParameterType>();

        protected override void OnInit()
        {
            AddParameterType(typeof(bool), (CommandParameter p, CommandReader r, out object v) =>
            {
                v = null;

                if (!r.ReadWord(out var word))
                {
                    r.Errors.Add($"Unable to parse True or False from '{r.CurrentArg}' for {p.Name}.");
                    return false;
                }

                v = string.Equals(word, "True", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(word, "Yes", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(word, "Enable", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(word, "T", StringComparison.OrdinalIgnoreCase);
                return true;
            });

            AddParameterType(typeof(byte), ReadNumber<byte>);
            AddParameterType(typeof(sbyte), ReadNumber<sbyte>);
            AddParameterType(typeof(ushort), ReadNumber<ushort>);
            AddParameterType(typeof(short), ReadNumber<short>);
            AddParameterType(typeof(uint), ReadNumber<uint>);
            AddParameterType(typeof(int), ReadNumber<int>);
            AddParameterType(typeof(float), ReadNumber<float>);
            AddParameterType(typeof(double), ReadNumber<double>);
            AddParameterType(typeof(ulong), ReadNumber<ulong>);
            AddParameterType(typeof(long), ReadNumber<long>);

            AddParameterType(typeof(string), (CommandParameter p, CommandReader r, out object v) =>
            {
                v = null;

                string value;
                if (p.WordLimit == 1)
                {
                    if (!r.ReadWord(out value))
                    {
                        r.Errors.Add($"Missing argument for {p.Name}.");
                        return false;
                    }
                }
                else
                {
                    if (!r.ReadSentence(out value, p.WordLimit))
                    {
                        r.Errors.Add($"Missing argument for {p.Name}.");
                        return false;
                    }
                }

                v = value;
                return true;
            });

            AddParameterType(typeof(Vector2), (CommandParameter p, CommandReader r, out object v) =>
            {
                v = null;
                var value = new Vector2();

                if (r.ArgsLeft == 0)
                {
                    r.Errors.Add($"An argument was missing for the x value for {p.Name}.");
                    return false;
                }
                if (!r.ReadNumber(out var num))
                {
                    r.Errors.Add($"Unable to parse '{r.CurrentArg}' to an x value for {p.Name}.");
                    return false;
                }
                value.x = (float)num;

                if (r.ArgsLeft == 0)
                {
                    r.Errors.Add($"An argument was missing for the y value for {p.Name}.");
                    return false;
                }
                if (!r.ReadNumber(out num))
                {
                    r.Errors.Add($"Unable to parse '{r.CurrentArg}' to a y value for {p.Name}.");
                    return false;
                }
                value.y = (float)num;

                v = value;
                return true;
            });

            AddParameterType(typeof(Vector3), (CommandParameter p, CommandReader r, out object v) =>
            {
                v = null;
                var value = new Vector3();

                if (r.ArgsLeft == 0)
                {
                    r.Errors.Add($"An argument was missing for the x value for {p.Name}.");
                    return false;
                }
                if (!r.ReadNumber(out var num))
                {
                    r.Errors.Add($"Unable to parse '{r.CurrentArg}' to an x value for {p.Name}.");
                    return false;
                }
                value.x = (float)num;

                if (r.ArgsLeft == 0)
                {
                    r.Errors.Add($"An argument was missing for the y value for {p.Name}.");
                    return false;
                }
                if (!r.ReadNumber(out num))
                {
                    r.Errors.Add($"Unable to parse '{r.CurrentArg}' to a y value for {p.Name}.");
                    return false;
                }
                value.y = (float)num;

                if (r.ArgsLeft == 0)
                {
                    r.Errors.Add($"An argument was missing for the z value for {p.Name}.");
                    return false;
                }
                if (!r.ReadNumber(out num))
                {
                    r.Errors.Add($"Unable to parse '{r.CurrentArg}' to a z value for {p.Name}.");
                    return false;
                }
                value.z = (float)num;

                v = value;
                return true;
            });

            AddParameterType(typeof(TimeSpan), (CommandParameter p, CommandReader r, out object v) =>
            {
                v = null;

                if (r.ArgsLeft == 0)
                {
                    r.Errors.Add($"An argument was missing for {p.Name}.");
                    return false;
                }
                if (!r.ReadNumber(out var num))
                {
                    r.Errors.Add($"Unable to parse '{r.CurrentArg}' to seconds.");
                    return false;
                }

                v = TimeSpan.FromSeconds(num);
                return true;
            });

            AddParameterType(typeof(DateTime), (CommandParameter p, CommandReader r, out object v) =>
            {
                v = null;

                if (r.ArgsLeft == 0)
                {
                    r.Errors.Add($"An argument was missing for {p.Name}.");
                    return false;
                }

                int startIndex = r.ArgIndex;
                string dateString = null;
                while (r.ReadWord(out var word))
                {
                    if (dateString.Length > 0)
                        dateString += " ";
                    dateString += word;

                    if (DateTime.TryParse(dateString, out DateTime date))
                    {
                        v = date;
                        return true;
                    }
                }

                r.ArgIndex = startIndex;
                r.Errors.Add($"Unable to read date from argument {r.ArgIndex} for {p.Name}.");

                return false;
            });

            AddCommands(typeof(CommandSystem).Assembly);
            AddCommands(Game?.GetType().Assembly);
        }

        protected override void OnStart()
        {
        }

        protected override void OnEnd()
        {
            _commands.Clear();
            _typeReaders.Clear();
        }

        private bool ReadNumber<T>(CommandParameter parameter, CommandReader reader, out object value)
        {
            value = null;

            if (reader.ArgsLeft == 0)
            {
                reader.Errors.Add($"An argument was missing for {parameter.Name}.");
                return false;
            }

            if (!reader.ReadNumber(out double v))
            {
                reader.Errors.Add($"Unable to parse '{reader.CurrentArg}' to a number.");
                return false;
            }

            value = Convert.ChangeType(v, typeof(T));
            return true;
        }

        public void AddCommands(Assembly assembly)
        {
            if (assembly == null)
                return;

            var types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];
                var staticMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                for (int j = 0; j < staticMethods.Length; j++)
                {
                    var method = staticMethods[j];
                    var attributes = method.GetCustomAttributes(false);
                    var cmd = attributes.FirstOrDefault(a => a is CommandAttribute);
                    if (cmd != null)
                    {
                        AddCommand(method, attributes, null);
                    }
                }
            }
        }

        public void AddCommands(object target)
        {
            if(target == null)
                return;

            var type = target.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int j = 0; j < methods.Length; j++)
            {
                var method = methods[j];

                var attributes = method.GetCustomAttributes(false);
                var cmd = attributes.FirstOrDefault(a => a is CommandAttribute);
                if (cmd != null)
                {
                    AddCommand(method, attributes, target);
                }
            }
        }

        public bool AddCommand(MethodInfo method, object[] attributes, object target)
        {
            if (target == null && !method.IsStatic)
            {
                Debug.LogError($"Unable to add command because it's not static. ({method.DeclaringType.Name}.{method.Name})");
                return false;
            }

            var name = method.Name;
            var aliases = new List<string>();
            var cmdParams = new List<CommandParameter>();

            if (_commands.TryGetValue(name, out var info) && !info.IsAlias)
                return false;

            _commands.Remove(name);

            info = new CommandInfo();
            info.Name = name;
            info.Method = method;
            info.Target = target;

            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] is AliasAttribute aliasAttribute)
                {
                    for (int j = 0; j < aliasAttribute.Aliases.Length; j++)
                    {
                        aliases.Add(aliasAttribute.Aliases[j]);
                    }
                }
            }

            info.Aliases = aliases.ToArray();

            var parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var cmdParam = new CommandParameter();
                cmdParam.Name = parameter.Name;
                cmdParam.Type = parameter.ParameterType;
                cmdParam.WordLimit = -1; // TODO: Get proper word limit.
                cmdParams.Add(cmdParam);
            }

            info.Parameters = cmdParams.ToArray();

            _commands.Add(name, info);

            for (int i = 0; i < aliases.Count; i++)
            {
                var alias = aliases[i];
                if(_commands.ContainsKey(alias))
                    continue;

                var aliasInfo = new CommandInfo();
                aliasInfo.Name = alias;
                aliasInfo.IsAlias = true;
                aliasInfo.Aliases = Array.Empty<string>();
                aliasInfo.Method = method;
                aliasInfo.Target = target;
                aliasInfo.Parameters = info.Parameters.ToArray();
                _commands.Add(alias, aliasInfo);
            }

            return true;
        }

        public void AddParameterType(Type type, ReadParameterMethod reader, params string[] syntax)
        {
            _typeReaders[type] = new CommandParameterType()
            {
                Type = type,
                Reader = reader,
                Syntax = syntax,
            };
        }

        // TODO: Only execute on the server.
        public bool ExecuteCommand(string command, Action<string> errorHandler = null)
        {
            if (!command.StartsWith("/"))
                return false;

            var args = command.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var name = args[0].Remove(0, 1);

            if (!_commands.TryGetValue(name, out var info))
                return false;

            var reader = new CommandReader(args);

            var parameters = info.Parameters;
            var values = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (_typeReaders.TryGetValue(parameter.Type, out var pInfo))
                {
                    if (!pInfo.Reader(parameter, reader, out var value))
                    {
                        for (int j = 0; j < reader.Errors.Count; j++)
                        {
                            // TODO: Send to player.
                            errorHandler?.Invoke(reader.Errors[j]);
                        }
                        return false;
                    }

                    values[i] = value;
                }
            }

            try
            {
                info.Method.Invoke(info.Target, values.Length > 0 ? values : null);
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                errorHandler?.Invoke($"An error occurred when executing the '/{info.Name}' command.");
                return false;
            }

            return true;
        }
    }
}
