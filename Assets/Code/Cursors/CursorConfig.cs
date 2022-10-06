using ConfigGeneration.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnitySystemFramework.Core;

namespace UnitySystemFramework.Cursors
{
    [Serializable]
    public struct CursorEntry
    {
        public string Name;
        public int Priority;
        public Vector2 Origin;
        public float FPS;
        public Texture2D[] Textures;
        public CursorMode Mode;
    }

    [CreateAssetMenu(fileName = "CursorConfig", menuName = "Configs/Cursor Config", order = 100)]
    public class CursorConfig : GameplayConfig, IGenerateConfig
    {
        public CursorEntry[] Cursors = null;

        public string ConfigName => "CursorKey";
        public string ConfigNamespace => $"{nameof(UnitySystemFramework)}.{nameof(Cursors)}";

        public IEnumerable<ConfigEntry> GetConfigEntries()
        {
            return Cursors.Select(c => new ConfigEntry(c.Name, c.Name));
        }

        void OnValidate()
        {
            ConfigHelper.MarkAsChanged(this);
        }
    }
}
