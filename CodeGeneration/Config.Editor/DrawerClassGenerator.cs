using JetBrains.Annotations;
using System.Text;

namespace ConfigGeneration.Configs
{
    public class DrawerClassGenerator
    {
        public string Namespace;
        public string ClassName;

        private int _indent;
        private readonly StringBuilder _sb = new StringBuilder();

        public DrawerClassGenerator()
        {
        }

        public DrawerClassGenerator(string @namespace, string className)
        {
            Namespace = @namespace;
            ClassName = className;
        }

        public void Reset(string @namespace, string className)
        {
            Namespace = @namespace;
            ClassName = className;
        }

        public override string ToString()
        {
            _sb.Length = 0;
            _indent = 0;

            AppendLine($"using UnitySystemFramework.Editor.Drawers;");
            AppendLine($"using System.Linq;");
            AppendLine($"using UnityEditor;");
            AppendLine();
            AppendLine($"namespace {Namespace}");
            using (Scope())
            {
                AppendLine($"[CustomPropertyDrawer(typeof({ClassName}))]");
                AppendLine($"public class {ClassName}Drawer : OptionsDrawer");
                using (Scope())
                {
                    AppendLine($"private string[] _entries;");
                    AppendLine($"protected override string[] Entries => _entries ?? (_entries = {ClassName}.All.Select(i => i.Key).OrderBy(k => k).Prepend(\"None\").ToArray());");
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
                if (semiColon)
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
