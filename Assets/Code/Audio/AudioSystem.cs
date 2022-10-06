using UnitySystemFramework.Core;
using UnitySystemFramework.Levels;
using UnitySystemFramework.Settings;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySystemFramework.Audio
{
    public class AudioSystem : BaseSystem
    {
        private struct MusicItem
        {
            public string Key;
            public bool Repeat;
        }

        private SettingsSystem _settingsSystem;

        private GameObject _sourceContainer;
        private readonly Queue<AudioSource> _sourcePool = new Queue<AudioSource>();
        private readonly List<AudioSource> _usedSources = new List<AudioSource>();
        private readonly Dictionary<string, AudioEntry> _settings = new Dictionary<string, AudioEntry>();

        private AudioSource _musicSource;
        private readonly Queue<MusicItem> _playlist = new Queue<MusicItem>();

        private float _masterVolume;
        private float _musicVolume;
        private float _soundVolume;

        private float _lastSourceFind;

#if !DONT_WANT_FMOD && FMOD
        private float _lastFMODUpdate;
        private FMOD.Studio.Bus _musicBus;
        private FMOD.Studio.Bus _soundBus;
#endif

        public string CurrentSong { get; private set; }

        [ApplyImmediately]
        [SettingRange(0f, 1f)]
        [Setting("Sound/Master Volume")]
        public float MasterVolume
        {
            get => _masterVolume;
            private set
            {
                SetMasterVolume(value);
                _masterVolume = value;
            }
        }

        [ApplyImmediately]
        [SettingRange(0f, 1f)]
        [Setting("Sound/Music Volume")]
        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                SetMusicVolume(value);
                _musicVolume = value;
            }
        }

        [ApplyImmediately]
        [SettingRange(0f, 1f)]
        [Setting("Sound/Sound Volume")]
        public float SoundVolume
        {
            get => _soundVolume;
            set
            {
                SetSoundVolume(value);
                _soundVolume = value;
            }
        }

        protected override void OnInit()
        {
            _settingsSystem = RequireSystem<SettingsSystem>();

            var settings = GetConfig<AudioConfig>();
            var clips = settings.Clips;
            for (int i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                _settings.Add(clip.Name, clip);
            }

            _masterVolume = PlayerPrefs.GetFloat("MasterVolume", settings.DefaultVolume);
            _musicVolume = PlayerPrefs.GetFloat("MusicVolume", settings.DefaultMusicVolume);
            _soundVolume = PlayerPrefs.GetFloat("SoundVolume", settings.DefaultSoundVolume);

            AddUpdate(OnUpdate);
            Subscribe<SceneLoadEvent>(OnSceneLoad);
        }

        protected override void OnStart()
        {
            _sourceContainer = new GameObject("AudioSources");
            Object.DontDestroyOnLoad(_sourceContainer);
            _musicSource = CreateSource(false);

            SetMasterVolume(_masterVolume);

            _settingsSystem.AddSettings(this);
            _settingsSystem.BuildMenu();
            _settingsSystem.UpdateSettings();

#if !DONT_WANT_FMOD && FMOD
            _musicBus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
            if(_musicBus == null)
                LogError("Music bus not found!");
            _soundBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
            if (_soundBus == null)
                LogError("Sound bus not found!");
#endif
        }

        private void OnSceneLoad(ref SceneLoadEvent evt)
        {
            SetSceneSourcesVolume();
        }

        private void OnUpdate()
        {
            for (int i = 0; i < _usedSources.Count; i++)
            {
                var source = _usedSources[i];
                if (!source.isPlaying)
                {
                    _sourcePool.Enqueue(source);
                    source.clip = null;
                    source.gameObject.name = "AudioSource";
                    //Object.Destroy(source.gameObject);
                    _usedSources.RemoveAt(i);
                    _sourceContainer.name = $"AudioSources ({_usedSources.Count})";
                    i--;
                }
            }

            if (!_musicSource.isPlaying && _playlist.Count > 0)
                PlayNextSong();

            // TODO: This is awful and does a find every second...
            if (Time.time - _lastSourceFind > 1f)
            {
                _lastSourceFind = Time.time;
                SetSceneSourcesVolume();
            }

#if !DONT_WANT_FMOD && FMOD
            if (Time.time - _lastFMODUpdate > 0.1f)
            {
                _lastFMODUpdate = Time.time;
                if (_musicBus != null)
                    _musicBus.setVolume(_musicVolume * _masterVolume);

                if (_soundBus != null)
                    _soundBus.setVolume(_soundVolume * _masterVolume);
            }
#endif
        }

        protected override void OnEnd()
        {
            while (_sourcePool.Count > 0)
                Object.Destroy(_sourcePool.Dequeue().gameObject);

            for (int i = 0; i < _usedSources.Count; i++)
                Object.Destroy(_usedSources[i].gameObject);

            _usedSources.Clear();
            RemoveUpdate(OnUpdate);
            Unsubscribe<SceneLoadEvent>(OnSceneLoad);
        }

        public void PauseAll()
        {
#if !DONT_WANT_FMOD && FMOD
            _musicBus.setPaused(true);
            _soundBus.setPaused(true);
#endif
            _musicSource.Pause();
            foreach (var soundSource in _usedSources)
            {
                soundSource.Pause();
            }
        }

        public void ResumeAll()
        {
#if !DONT_WANT_FMOD && FMOD
            _musicBus.setPaused(false);
            _soundBus.setPaused(false);
#endif
            _musicSource.UnPause();
            foreach (var soundSource in _usedSources)
            {
                soundSource.UnPause();
            }
        }

        /// <summary>
        /// Sets the master volume for the game. Keep between 0 and 1.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            _masterVolume = volume;
            PlayerPrefs.SetFloat("MasterVolume", volume);

            SetMusicVolume(_musicVolume);
            SetSoundVolume(_soundVolume);

            _settingsSystem.UpdateSetting("Sound/Master Volume");
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = volume;
            PlayerPrefs.SetFloat("MusicVolume", volume);

            _musicSource.volume = _musicVolume * _masterVolume;

            _settingsSystem.UpdateSetting("Sound/Music Volume");
        }

        public void SetSoundVolume(float volume)
        {
            _soundVolume = volume;
            PlayerPrefs.SetFloat("SoundVolume", volume);

            SetSceneSourcesVolume();

            _settingsSystem.UpdateSetting("Sound/Sound Volume");
        }

        private void SetSceneSourcesVolume()
        {
            var audioSources = Object.FindObjectsOfType<AudioSource>();
            foreach (var source in audioSources)
            {
                if (source == _musicSource)
                    continue;

                source.volume = _soundVolume * _masterVolume;
            }
        }

        private AudioSource CreateSource(bool pool = true)
        {
            if (pool && _sourcePool.Count > 0)
            {
                var s = _sourcePool.Dequeue();
                _usedSources.Add(s);
                _sourceContainer.name = $"AudioSources ({_usedSources.Count})";
                return s;
            }

            var go = new GameObject("AudioSource");
            Object.DontDestroyOnLoad(go);
            go.transform.parent = _sourceContainer.transform;
            var source = go.AddComponent<AudioSource>();
            if (pool)
            {
                _usedSources.Add(source);
                _sourceContainer.name = $"AudioSources ({_usedSources.Count})";
            }
            return source;
        }

        public void PlaySound(AudioKey key)
        {
            PlaySound(key, Vector3.zero, -1);
        }

        public void PlaySound(AudioKey key, Vector3 position, float range)
        {
            if (_settings.TryGetValue(key, out var setting))
            {
                if (range < 0)
                    range = float.MaxValue;

                var source = CreateSource();
                source.name = $"AudioSource [{key}]";
                source.volume = MasterVolume * SoundVolume;
                source.transform.position = position;
                source.minDistance = 0;
                source.maxDistance = range;
                var clip = setting.Clips[Random.Range(0, setting.Clips.Length)];
                source.PlayOneShot(clip.Clip, clip.BaseVolume);
            }
        }

        public void StopSounds()
        {
            for (int i = 0; i < _usedSources.Count; i++)
            {
                var source = _usedSources[i];
                source.Stop();
                source.clip = null;
                _sourcePool.Enqueue(source);
                source.gameObject.name = "AudioSource";
                //Object.Destroy(source.gameObject);
                _usedSources.RemoveAt(i);
                i--;
            }
        }

        public void PlayNextSong()
        {
            if (_playlist.Count == 0)
                return;

            var song = _playlist.Dequeue();
            if (!_settings.TryGetValue(song.Key, out var setting))
            {
                PlayNextSong();
                return;
            }

            var clipSetting = setting.Clips[Random.Range(0, setting.Clips.Length)];
            CurrentSong = song.Key;
            _musicSource.name = $"AudioSource [{song.Key}]";
            _musicSource.volume = MasterVolume * MusicVolume * clipSetting.BaseVolume;
            _musicSource.minDistance = 0;
            _musicSource.maxDistance = float.MaxValue;
            _musicSource.clip = clipSetting.Clip;
            _musicSource.loop = song.Repeat;
            _musicSource.Play();
        }

        public void QueueMusic(AudioKey key, bool repeat)
        {
            _playlist.Enqueue(new MusicItem()
            {
                Key = key,
                Repeat = repeat,
            });

            if(!_musicSource.isPlaying)
                PlayNextSong();
        }

        public void ClearPlaylist()
        {
            _playlist.Clear();
            CurrentSong = null;
            _musicSource.Stop();
            _musicSource.clip = null;
            _musicSource.loop = false;
        }
    }
}
