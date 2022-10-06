using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ConfigGeneration.Configs
{
    [InitializeOnLoad]
    public class ConfigGenerator : AssetPostprocessor
    {
        public const string GENERATED_CONFIG_DIR = "Assets/Generated/Configs";
        public const string GENERATED_DRAWER_DIR = "Assets/Generated/Configs/Editor";

        private static readonly Dictionary<string, string> _currentConfigAssets = new Dictionary<string, string>();

        static ConfigGenerator()
        {
            var configs = Resources.LoadAll<ScriptableObject>("");

            var gen = new ConfigClassGenerator();
            var drawer = new DrawerClassGenerator();
            foreach (var loaded in configs)
            {
                if (!(loaded is IGenerateConfig configList))
                    continue;

                GenerateConfig(gen, drawer, configList);
            }
        }

        [MenuItem("USF/Configs/Force Generate")]
        private static void ForceGenerate()
        {
            var configs = Resources.LoadAll<ScriptableObject>("");

            var gen = new ConfigClassGenerator();
            var drawer = new DrawerClassGenerator();
            foreach (var loaded in configs)
            {
                if (!(loaded is IGenerateConfig configList))
                    continue;

                GenerateConfig(gen, drawer, configList);
            }
        }

        private static void GenerateConfig(ConfigClassGenerator gen, DrawerClassGenerator drawer, IGenerateConfig configList)
        {
            string name = configList.ConfigName;
            string ns = configList.ConfigNamespace;
            IEnumerable<ConfigEntry> list = configList.GetConfigEntries();

            gen.Reset(ns, name);
            drawer.Reset(ns, name);
            foreach (var item in list)
            {
                if (string.IsNullOrWhiteSpace(item.Key))
                    continue;

                gen.Add(ConfigHelper.EscapeKey(item.Key), item.Value, item.Doc);
            }

            var file = $"{GENERATED_CONFIG_DIR}/{name}.cs";
            _currentConfigAssets[AssetDatabase.GetAssetPath(configList as Object)] = file;
            WriteSource(gen.ToString(), GENERATED_CONFIG_DIR, file);

            file = $"{GENERATED_DRAWER_DIR}/{name}Drawer.cs";
            WriteSource(drawer.ToString(), GENERATED_DRAWER_DIR, file);
        }

        private static void WriteSource(string content, string dir, string file)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(file))
            {
                var existing = File.ReadAllText(file);
                if (content.GetHashCode() == existing.GetHashCode())
                    return;
                File.Delete(file);
            }

            File.WriteAllText(file, content);
            AssetDatabase.ImportAsset(file, ImportAssetOptions.Default);
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var gen = new ConfigClassGenerator();
            var drawer = new DrawerClassGenerator();
            foreach (var config in ConfigHelper.GetChangedConfigs())
            {
                GenerateConfig(gen, drawer, config);
            }
            ConfigHelper.ClearChangedConfigs();
        }
    }
}
