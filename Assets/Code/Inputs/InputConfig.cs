using ConfigGeneration.Configs;
using UnitySystemFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySystemFramework.Inputs
{
    [Serializable]
    public struct InputEntry
    {
        public string Name;
        public string[] Aliases;
        public string DisplayName;
        public string Description;
        public KeyCode Key;
        public string[] Scopes;
    }

    [Serializable]
    public struct InputScheme
    {
        public string Name;
        public InputEntry[] Inputs;
    }

    [Serializable]
    public struct InputScopeSetting
    {
        public string ScopeName;
        public string[] DisabledBy;
    }

    [CreateAssetMenu(fileName = "InputConfig", menuName = "Configs/Input Config", order = 100)]
    public class InputConfig : GameplayConfig, IGenerateConfig
    {
        public LayerMask MouseLayers;
        public float DefaultKeyInterval = 0.2f;
        public InputKey HorizontalAxis;
        public InputKey VerticalAxis;
        public InputKey ScrollAxis;
        public InputScheme[] Schemes = null;

        public string ConfigName => "InputKey";
        public string ConfigNamespace => $"{nameof(UnitySystemFramework)}.{nameof(Inputs)}";

        public IEnumerable<ConfigEntry> GetConfigEntries()
        {
            var entries = Schemes.SelectMany(s => s.Inputs).Select(i => new ConfigEntry(i.Name, i.Name, i.Description));
#if UNITY_EDITOR
            var inputManager = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset");
            var obj = new UnityEditor.SerializedObject(inputManager);
            var axes = obj.FindProperty("m_Axes");

            for (int i = 0; i < axes.arraySize; i++)
            {
                var axis = axes.GetArrayElementAtIndex(i);
                var axisName = axis.FindPropertyRelative("m_Name").stringValue;
                entries = entries.Append(new ConfigEntry($"UNITY_{axisName}", axisName));
            }
#endif
            return entries;
        }

        void OnValidate()
        {
            for (var i = 0; i < Schemes.Length; i++)
            {
                var scheme = Schemes[i];
                for (var j = 0; j < scheme.Inputs.Length; j++)
                {
                    var input = scheme.Inputs[j];
                    input.Name = ConfigHelper.EscapeKey(input.Name);
                    if (string.IsNullOrEmpty(input.DisplayName))
                        input.DisplayName = input.Name;
                    scheme.Inputs[j] = input;
                }
                Schemes[i] = scheme;
            }

            ConfigHelper.MarkAsChanged(this);
        }
    }
}
