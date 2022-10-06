using System.Linq;
using UnitySystemFramework.Core;
using UnitySystemFramework.Settings;
using UnityEngine;

namespace UnitySystemFramework.Graphics
{
    public class QualitySystem : BaseSystem
    {
        private SettingsSystem _settingsSystem;

        private int _qualityIndex;

        [ConfirmAfterApply]
        [Setting("Graphics/Quality Level", "The quality level of the game.", 10)]
        private int QualityIndex
        {
            get => _qualityIndex;
            set
            {
                QualitySettings.SetQualityLevel(_qualityIndex, true);
                _qualityIndex = value;
            }
        }

        [Options("Graphics/Quality Level")]
        private object[] GetQualityOptions()
        {
            return QualitySettings.names.Select(n => (object)n).ToArray();
        }

        protected override void OnInit()
        {
            _settingsSystem = RequireSystem<SettingsSystem>();
        }

        protected override void OnStart()
        {
            _qualityIndex = QualitySettings.GetQualityLevel();
            _settingsSystem.AddSettings(this);
            _settingsSystem.BuildMenu();
            _settingsSystem.UpdateSettings();
        }

        protected override void OnEnd()
        {
        }
    }
}
