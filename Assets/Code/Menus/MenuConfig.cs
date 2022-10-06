using ConfigGeneration.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnitySystemFramework.Core;

namespace UnitySystemFramework.Menus
{
    [Serializable]
    public struct MenuEntry
    {
        public string Name;
        public int SortOrder;
        public GameObject Prefab;
        public bool CursorIsVisible;
        public CursorLockMode CursorMode;
    }

    [CreateAssetMenu(fileName = "MenuConfig", menuName = "Configs/Menu Config", order = 100)]
    public class MenuConfig : GameplayConfig, IGenerateConfig
    {
        public MenuEntry[] Menus = null;

        public string ConfigName => "MenuKey";
        public string ConfigNamespace => $"{nameof(UnitySystemFramework)}.{nameof(Menus)}";

        public IEnumerable<ConfigEntry> GetConfigEntries()
        {
            return Menus.Select(m => new ConfigEntry(m.Name, m.Name));
        }

        void OnValidate()
        {
            ConfigHelper.MarkAsChanged(this);
        }
    }
}
