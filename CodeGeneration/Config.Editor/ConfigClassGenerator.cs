using JetBrains.Annotations;
using System.Collections.Generic;
using System.Text;

namespace ConfigGeneration.Configs
{
    public class ConfigClassGenerator
    {
        public struct Entry
        {
            public string Key;
            public object Value;
            public string Doc;

            public Entry(string key, object value, string doc = "")
            {
                Key = key;
                Value = value;
                Doc = doc;
            }
        }

        public string Namespace;
        public string ClassName;
        public List<Entry> Entries = new List<Entry>();

        private readonly HashSet<string> _names = new HashSet<string>();
        private int _indent;
        private readonly StringBuilder _sb = new StringBuilder();

        public ConfigClassGenerator()
        {
        }

        public ConfigClassGenerator(string @namespace, string className)
        {
            Namespace = @namespace;
            ClassName = className;
        }

        public void Add(string key, object value, string doc = "")
        {
            if (!_names.Contains(key))
            {
                _names.Add(key);
                Entries.Add(new Entry(key, value, doc));
            }
        }

        public void Reset(string @namespace, string className)
        {
            Namespace = @namespace;
            ClassName = className;
            _names.Clear();
            Entries.Clear();
        }

        public override string ToString()
        {
            _sb.Length = 0;
            _indent = 0;
            AppendLine("using System;");
            AppendLine("using System.Collections.Generic;");
            AppendLine("using System.Linq;");
            AppendLine("using UnityEngine;");
            AppendLine();
            AppendLine($"namespace {Namespace}");
            using(Scope())
            {
                AppendLine($"/// <summary>");
                AppendLine($"/// A list that is generated from a ScriptableObject that implements IGenerateConfig.");
                AppendLine($"/// </summary>");
                AppendLine($"[Serializable]");
                AppendLine($"public struct {ClassName} : IEquatable<{ClassName}>");
                using(Scope())
                {
                    AppendLine($"private {ClassName}(string key)");
                    using (Scope())
                    {
                        AppendLine($"Key = key;");
                    }
                    AppendLine();
                    AppendLine($"/// <summary>");
                    AppendLine($"/// The key for this entry.");
                    AppendLine($"/// </summary>");
                    AppendLine($"[SerializeField]");
                    AppendLine($"public string Key;");
                    AppendLine();
                    using (Region("Functions"))
                    {
                        AppendLine($"/// <summary>");
                        AppendLine($"/// Gets the value of an InputKey using the field name. (Key)");
                        AppendLine($"/// </summary>");
                        AppendLine($"public static object Get(string key)");
                        using (Scope())
                        {
                            AppendLine("if(key == null)");
                            using(Indent())
                                AppendLine("return null;");
                            AppendLine("_valueLookup.TryGetValue(key, out var value);");
                            AppendLine("return value;");
                        }

                        AppendLine();
                        AppendLine($"/// <summary>");
                        AppendLine($"/// Determines if the key provided is a valid entry.");
                        AppendLine($"/// </summary>");
                        AppendLine($"public static bool IsValid(string key)");
                        using (Scope())
                        {
                            AppendLine("if(key == null)");
                            using (Indent())
                                AppendLine("return false;");
                            AppendLine($"return _valueLookup.ContainsKey(key);");
                        }

                        AppendLine();
                        AppendLine($"/// <summary>");
                        AppendLine($"/// Determines if the value of this entry is the type specified.");
                        AppendLine($"/// </summary>");
                        AppendLine($"public bool Is<T>()");
                        using (Scope())
                        {
                            AppendLine($"return Get(Key) is T;");
                        }

                        AppendLine();
                        AppendLine();
                        AppendLine($"/// <summary>");
                        AppendLine($"/// Determines if this {ClassName} is valid.");
                        AppendLine($"/// </summary>");
                        AppendLine($"public bool IsValid()");
                        using (Scope())
                        {
                            AppendLine($"return IsValid(Key);");
                        }
                    }
                    AppendLine();
                    using (Region("Entries"))
                    {
                        AppendLine($"/// <summary>");
                        AppendLine($"/// A default invalid key.");
                        AppendLine($"/// </summary>");
                        AppendLine($"public static readonly {ClassName} Invalid = default;");
                        AppendLine();
                        AppendLine($"/// <summary>");
                        AppendLine($"/// Returns a new list with all of the entries from {ClassName}.");
                        AppendLine($"/// </summary>");
                        AppendLine($"public static readonly IList<KeyValuePair<string, object>> All;");
                        AppendLine();
                        for (int i = 0; i < Entries.Count; i++)
                        {
                            var key = Entries[i].Key;
                            var value = Entries[i].Value;
                            var doc = Entries[i].Doc;
                            if (!string.IsNullOrWhiteSpace(doc))
                            {
                                AppendLine($"/// <summary>");
                                AppendLine($"/// {doc}");
                                AppendLine($"/// </summary>");
                            }

                            AppendLine($"public static readonly {ClassName} {key} = new {ClassName}(nameof({key}));");
                            if(i < Entries.Count - 1)
                                AppendLine();
                        }
                    }
                    AppendLine();
                    using (Region("Lookup"))
                    {
                        AppendLine($"private static readonly Dictionary<string, object> _valueLookup;");
                        AppendLine();
                        AppendLine($"static {ClassName}()");
                        using (Scope())
                        {
                            AppendLine($"_valueLookup = new Dictionary<string, object>()");
                            using (Scope(semiColon: true))
                            {
                                for (int i = 0; i < Entries.Count; i++)
                                {
                                    var key = Entries[i].Key;
                                    var value = Entries[i].Value;
                                    AppendLine($@"{{nameof({key}), {GetValueString(value)}}},");
                                }
                            }

                            AppendLine("All = _valueLookup.ToList();");
                        }
                    }
                    AppendLine();
                    using (Region("Operators"))
                    {
                        AppendLine($"public static implicit operator {ClassName}(string key)");
                        using (Scope())
                        {
                            AppendLine($"return new {ClassName}(key);");
                        }

                        AppendLine();
                        AppendLine($"public static implicit operator bool({ClassName} key)");
                        using (Scope())
                        {
                            AppendLine($"return (bool) Get(key.Key);");
                        }

                        AppendLine();
                        AppendLine($"public static implicit operator char({ClassName} key)");
                        using (Scope())
                        {
                            AppendLine($"return (char) Get(key.Key);");
                        }

                        AppendLine();
                        AppendLine($"public static implicit operator byte({ClassName} key)");
                        using (Scope())
                        {
                            AppendLine($"return (byte) Get(key.Key);");
                        }

                        AppendLine();
                        AppendLine($"public static implicit operator sbyte({ClassName} key)");
                        using (Scope())
                        {
                            AppendLine($"return (sbyte) Get(key.Key);");
                        }

                        AppendLine();
                        AppendLine($"public static implicit operator short({ClassName} key)");
                        using (Scope())
                        {
                            AppendLine($"return (short) Get(key.Key);");
                        }

                        AppendLine();
                        AppendLine($"public static implicit operator ushort({ClassName} key)");
                        using (Scope())
                        {
                            AppendLine($"return (ushort) Get(key.Key);");
                        }

                        AppendLine();
                        AppendLine($"public static implicit operator int({ClassName} key)");
                        using (Scope())
                        {
                            AppendLine($"return (int) Get(key.Key);");
                        }

                        AppendLine();
                        AppendLine($"public static implicit operator uint({ClassName} key)");
                        using (Scope())
                        {
                            AppendLine($"return (uint) Get(key.Key);");
                        }

                        AppendLine();
                        AppendLine($"public static implicit operator float({ClassName} key)");
                        using (Scope())
                        {
                            AppendLine($"return (float) Get(key.Key);");
                        }

                        AppendLine();
                        AppendLine($"public static implicit operator double({ClassName} key)");
                        using (Scope())
                        {
                            AppendLine($"return (double) Get(key.Key);");
                        }

                        AppendLine();
                        AppendLine($"public static implicit operator long({ClassName} key)");
                        using (Scope())
                        {
                            AppendLine($"return (long) Get(key.Key);");
                        }

                        AppendLine();
                        AppendLine($"public static implicit operator ulong({ClassName} key)");
                        using (Scope())
                        {
                            AppendLine($"return (ulong) Get(key.Key);");
                        }

                        AppendLine();
                        AppendLine($"public static implicit operator decimal({ClassName} key)");
                        using (Scope())
                        {
                            AppendLine($"return (decimal) Get(key.Key);");
                        }

                        AppendLine();
                        AppendLine($"public static implicit operator string({ClassName} key)");
                        using (Scope())
                        {
                            AppendLine($"return (string) Get(key.Key);");
                        }

                        AppendLine();
                        AppendLine($"public bool Equals({ClassName} other)");
                        using (Scope())
                        {
                            AppendLine($"return Key == other.Key;");
                        }

                        AppendLine();
                        AppendLine($"public override bool Equals(object obj)");
                        using (Scope())
                        {
                            AppendLine($"return obj is {ClassName} other && Equals(other);");
                        }

                        AppendLine();
                        AppendLine($"public override int GetHashCode()");
                        using (Scope())
                        {
                            AppendLine($"if(Key == null)");
                            using (Indent())
                                AppendLine($"return 0;");
                            AppendLine($"var value = Get(Key);");
                            AppendLine($"return (value != null ? value.GetHashCode() : 0);");
                        }

                        AppendLine();
                        AppendLine($"public override string ToString()");
                        using (Scope())
                        {
                            AppendLine($"if(Key == null)");
                            using (Indent())
                                AppendLine($"return default;");
                            AppendLine($"return Get(Key) + \"\";");
                        }

                        AppendLine();
                        AppendLine($"public static bool operator ==({ClassName} a, {ClassName} b)");
                        using (Scope())
                        {
                            AppendLine($"return a.Equals(b);");
                        }

                        AppendLine();
                        AppendLine($"public static bool operator !=({ClassName} a, {ClassName} b)");
                        using (Scope())
                        {
                            AppendLine($"return !a.Equals(b);");
                        }
                    }
                }
            }

            return _sb.ToString();
        }

        [StringFormatMethod("value")]
        private void AppendLine(string value = "", params string[] args)
        {
            _sb.AppendLine($"{GetIndent()}{value}");
        }

        [StringFormatMethod("value")]
        private void Append(string value = "", params string[] args)
        {
            _sb.Append($"{GetIndent()}{value}");
        }

        private string GetIndent()
        {
            return new string(' ', _indent * 4);
        }

        private Indenter Scope(string closingName = null, bool semiColon = false)
        {
            AppendLine("{");
            _indent++;
            return new Indenter(() =>
            {
                _indent--;
                Append("}");
                if(semiColon)
                    _sb.Append(";");
                if (!string.IsNullOrWhiteSpace(closingName))
                    _sb.Append($" // {closingName}");
                _sb.AppendLine();
            });
        }

        private Indenter Region(string name)
        {
            AppendLine($"#region {name}");
            AppendLine();
            return new Indenter(() =>
            {
                AppendLine();
                AppendLine($"#endregion // {name}");
            });
        }

        private Indenter Indent()
        {
            _indent++;
            return new Indenter(() =>
            {
                _indent--;
            });
        }

        private string GetTypeString(object value)
        {
            if (value is string)
                return "string";
            if (value is bool)
                return "bool";
            if (value is char)
                return "char";
            if (value is byte)
                return "byte";
            if (value is sbyte)
                return "sbyte";
            if (value is short)
                return "short";
            if (value is ushort)
                return "ushort";
            if (value is int)
                return "int";
            if (value is uint)
                return "uint";
            if (value is long)
                return "long";
            if (value is ulong)
                return "ulong";
            if (value is float)
                return "float";
            if (value is double)
                return "double";
            if (value is decimal)
                return "decimal";
            return "object";
        }

        private string GetValueString(object value)
        {
            if (value is string s)
                return $"\"{s}\"";
            if (value is char c)
                return $"'{c}'";
            return $"{value}";
        }
    }
}
