using UnitySystemFramework.Core;
using UnitySystemFramework.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnitySystemFramework.Graphics
{
    public class ScreenSystem : BaseSystem
    {
        private struct Res : IEquatable<Res>
        {
            public Res(int width, int height)
            {
                Width = width;
                Height = height;
            }

            public int Width;
            public int Height;

            public bool Equals(Res other)
            {
                return Width == other.Width && Height == other.Height;
            }

            public override bool Equals(object obj)
            {
                return obj is Res other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Width * 397) ^ Height;
                }
            }
        }

        private SettingsSystem _settingsSystem;

        private int _resolutionIndex;
        private FullScreenMode _fullScreenMode;
        private List<Res> _resolutions;
        private Res _currentResolution;
        private bool _ended;

        [ConfirmAfterApply]
        [Setting("Graphics/Resolution", "How many pixels wide and tall to make the game window.")]
        private int ResolutionIndex
        {
            get => _resolutionIndex;
            set
            {
                var resolution = _resolutions[value];

                var currentResolution = Screen.currentResolution;
                if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
                {
                    // Find the highest refresh rate for this resolution.
                    currentResolution = Screen.resolutions.LastOrDefault(r => r.width == resolution.Width && r.height == resolution.Height);
                    if (Equals(currentResolution, default))
                        currentResolution = Screen.currentResolution;
                }

                Screen.SetResolution(resolution.Width, resolution.Height, Screen.fullScreen, currentResolution.refreshRate);
                _resolutionIndex = value;
            }
        }

        [ConfirmAfterApply]
        [Setting("Graphics/Full Screen Mode", "The type of full screen.")]
        public FullScreenMode FullscreenMode
        {
            get => _fullScreenMode;
            set
            {
                if(Screen.fullScreenMode != value)
                    Screen.fullScreenMode = value;
                _fullScreenMode = value;
            }
        }

        protected override void OnInit()
        {
            _settingsSystem = RequireSystem<SettingsSystem>();
            QueueUpdate(OnUpdate, count:-1, interval:0.5f);
        }

        protected override void OnStart()
        {
            _resolutions = Screen.resolutions.Select(r => new Res(r.width, r.height)).Distinct().ToList();
            _currentResolution = new Res(Screen.width, Screen.height);
            _resolutionIndex = _resolutions.IndexOf(_currentResolution);
            if (_resolutionIndex < 0)
            {
                _resolutionIndex = _resolutions.Count;
                _resolutions.Add(_currentResolution);
            }

            _fullScreenMode = Screen.fullScreenMode;

            _settingsSystem.AddSettings(this);

            foreach (var resolution in _resolutions)
            {
                _settingsSystem.AddSettingOption("Graphics/Resolution", $"{resolution.Width} x {resolution.Height}");
            }
            
            _settingsSystem.BuildMenu();
            _settingsSystem.UpdateSettings();
        }

        protected override void OnEnd()
        {
            _ended = true;
        }

        private bool OnUpdate()
        {
            if (_ended)
                return false;

            if (Screen.fullScreenMode != _fullScreenMode)
            {
                _fullScreenMode = Screen.fullScreenMode;
                _settingsSystem.UpdateSetting("Graphics/FullScreen");
            }

            if (Screen.width != _currentResolution.Width || Screen.height != _currentResolution.Height)
            {
                if(!Screen.resolutions.Any(r => r.width == _currentResolution.Width && r.height == _currentResolution.Height))
                {
                    _resolutions.Remove(_currentResolution);
                    _settingsSystem.RemoveSettingOption("Graphics/Resolution", $"{_currentResolution.Width} x {_currentResolution.Height}");
                }
                _currentResolution = new Res(Screen.width, Screen.height);
                _resolutionIndex = _resolutions.IndexOf(_currentResolution);
                if (_resolutionIndex < 0)
                {
                    _settingsSystem.AddSettingOption("Graphics/Resolution", $"{_currentResolution.Width} x {_currentResolution.Height}");
                    _resolutionIndex = _resolutions.Count;
                    _resolutions.Add(_currentResolution);
                }
                _settingsSystem.UpdateSetting("Graphics/Resolution");
            }

            return true;
        }
    }
}
