using System.Collections.Generic;
using UnityEngine;

namespace UnitySystemFramework.Core
{
    public class GameplayConfig : ScriptableObject
    {
    }

    public partial interface IGame
    {
        /// <summary>
        /// Gets the config by type.
        /// </summary>
        T GetConfig<T>() where T : GameplayConfig;
    }

    public static partial class Game
    {
    }

    public partial class BaseGame
    {
        private readonly Dictionary<TypeID, GameplayConfig> _configs = new Dictionary<TypeID, GameplayConfig>();

        private void InitializeSettings()
        {
            var assets = Resources.LoadAll<GameplayConfig>("");

            for (int i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                if(asset is GameplayConfig config)
                    _configs.Add(config.GetType().GetTypeID(), config);
            }
        }

        private void UninitializeSettings()
        {
            _configs.Clear();
        }

        /// <inheritdoc cref="IGame.GetConfig{T}"/>
        public T GetConfig<T>() where T : GameplayConfig
        {
            var type = TypeID<T>.ID;
            if (_configs.TryGetValue(type, out var config))
                return config as T;

            LogError($"Could not find config for {type.Name}. Make sure the resource exists.");
            return default;
        }
    }
}
