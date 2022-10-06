using System;
using UnitySystemFramework.Core;
using System.Collections.Generic;
using UnitySystemFramework.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace UnitySystemFramework.Levels
{
    public class LevelSystem : BaseSystem
    {
        private readonly Dictionary<string, LevelEntry> _settingsByName = new Dictionary<string, LevelEntry>();
        private readonly Dictionary<string, LevelEntry> _settingsByScene = new Dictionary<string, LevelEntry>();

        public IReadOnlyList<Level> Levels { get; private set; }
        public Level CurrentLevel { get; private set; }

        private AsyncOperation _currentLevelAsync;

        protected override void OnInit()
        {
            var settings = GetConfig<LevelConfig>();
            var levelSettings = settings.Levels;
            var levels = new List<Level>();
            for (int i = 0; i < levelSettings.Length; i++)
            {
                var setting = levelSettings[i];
                if (string.IsNullOrWhiteSpace(setting.Name) || string.IsNullOrWhiteSpace(setting.SceneName))
                    continue;

                _settingsByName.Add(levelSettings[i].Name, levelSettings[i]);
                _settingsByScene.Add(levelSettings[i].SceneName, levelSettings[i]);
                levels.Add(new Level()
                {
                    Name = setting.Name,
                    SceneName = setting.SceneName,
                    MaxPlayers = setting.MaxPlayers,
                    MaxTeamSize = setting.MaxTeamSize,
                    Description = setting.Description,
                });
            }

            Levels = levels.AsReadOnly();
        }

        protected override void OnStart()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnload;
            var scene = SceneManager.GetActiveScene();

            if (scene.isLoaded)
            {
                CallEvent(new SceneLoadEvent()
                {
                    Scene = scene,
                });

                if (_settingsByScene.TryGetValue(scene.name, out var settings))
                {
                    CurrentLevel = new Level()
                    {
                        Name = settings.Name,
                        SceneName = settings.SceneName,
                        MaxPlayers = settings.MaxPlayers,
                        MaxTeamSize = settings.MaxTeamSize,
                        Description = settings.Description,
                        IsLoaded = true,
                    };
                    CallEvent(new LevelLoadEvent()
                    {
                        Level = CurrentLevel,
                    });
                }
            }
        }

        protected override void OnEnd()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnload;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CallEvent(new SceneLoadEvent()
            {
                Scene = scene,
            });

            if (!_settingsByScene.TryGetValue(scene.name, out var setting))
                return;

            CurrentLevel = new Level()
            {
                Name = setting.Name,
                SceneName = setting.SceneName,
                MaxPlayers = setting.MaxPlayers,
                MaxTeamSize = setting.MaxTeamSize,
                Description = setting.Description,
                IsLoaded = true,
            };
            CallEvent(new LevelLoadEvent()
            {
                Level = CurrentLevel,
            });
            CallEvent(new OtherLoadLevelEvent()
            {
                Level = CurrentLevel,
            });
        }

        private void OnSceneUnload(Scene scene)
        {
            if (IsSceneALevel(scene.name))
            {
                CallEvent(new LevelUnloadEvent()
                {
                    Level = CurrentLevel,
                });
                CurrentLevel = Level.Default;
            }
            CallEvent(new SceneUnloadEvent()
            {
                Scene = scene,
            });
        }

        public Level LoadLevel(string name)
        {
            if (!_settingsByName.TryGetValue(name, out var setting))
                return default;

            if (!Equals(CurrentLevel, Level.Default))
            {
                CallEvent(new LevelUnloadEvent()
                {
                    Level = CurrentLevel,
                });
            }

            // Keep variable because it's used in a lambda down below.
            var level = new Level()
            {
                Name = setting.Name,
                SceneName = setting.SceneName,
                MaxPlayers = setting.MaxPlayers,
                MaxTeamSize = setting.MaxTeamSize,
                Description = setting.Description,
                IsLoaded = false,
            };
            CurrentLevel = level;

            // TODO: May no longer be a limitation.
            // Load Single because unity cannot load some things additive such as nav mesh or occlusion culling.
            _currentLevelAsync = SceneManager.LoadSceneAsync(setting.SceneName, LoadSceneMode.Single);

            AddUpdate(LevelLoadUpdate);

            CallEvent(new LevelBeginLoadEvent()
            {
                Level = level,
                Async = _currentLevelAsync,
            });

            return CurrentLevel;
        }

        private void LevelLoadUpdate()
        {
            var level = CurrentLevel;
            if (_currentLevelAsync.isDone && !CurrentLevel.IsLoaded)
            {
                level.Progress = 1f;
                CurrentLevel = level;
                RemoveUpdate(LevelLoadUpdate);
                return;
            }
            level.Progress = _currentLevelAsync.progress;
            CurrentLevel = level;
        }

        public bool IsSceneALevel(string sceneName)
        {
            return _settingsByScene.ContainsKey(sceneName);
        }

        public Level GetLevelByScene(string sceneName)
        {
            if (sceneName == CurrentLevel.SceneName)
                return CurrentLevel;

            _settingsByScene.TryGetValue(sceneName, out var level);
            return new Level()
            {
                Name = level.Name,
                SceneName = level.SceneName,
                MaxPlayers = level.MaxPlayers,
                MaxTeamSize = level.MaxTeamSize,
                Description = level.Description,
            };
        }

        public Level GetLevel(string name)
        {
            if (name == CurrentLevel.Name)
                return CurrentLevel;

            _settingsByName.TryGetValue(name, out var level);
            return new Level()
            {
                Name = level.Name,
                SceneName = level.SceneName,
                MaxPlayers = level.MaxPlayers,
                MaxTeamSize = level.MaxTeamSize,
                Description = level.Description,
            };
        }

        public Sprite GetMapImage(string name)
        {
            if (!_settingsByName.TryGetValue(name, out var level))
                return null;
            return level.Image;
        }

        public Level GetRandomMultiplayerMap(int currentPlayers = -1)
        {
            var levels = Levels;
            var set = new HashSet<int>();
            int randomIndex = Random.Range(0, levels.Count);
            while (currentPlayers >= 0 && levels[randomIndex].MaxPlayers < Math.Max(currentPlayers, 2))
            {
                set.Add(randomIndex);
                if (set.Count == levels.Count)
                    return default;

                randomIndex = Random.Range(0, levels.Count);
            }

            return levels[randomIndex];
        }
    }
}
