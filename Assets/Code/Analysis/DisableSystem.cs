using System.Collections.Generic;
using UnitySystemFramework.Commands;
using UnitySystemFramework.Core;

namespace UnitySystemFramework.Analysis
{
    public class DisableSystem : BaseSystem
    {
        private static DisableSystem _instance;

        /// <summary>
        /// Returns true if the specified key is enabled.
        /// </summary>
        public static new bool IsEnabled(string key)
        {
            if (_instance == null)
                return true;
            if (!_instance._enableLookup.TryGetValue(key, out var enabled))
                return true;
            return enabled;
        }

        private readonly Dictionary<string, bool> _enableLookup = new Dictionary<string, bool>();

        protected override void OnInit()
        {
            _instance = this;
            Subscribe<DisableEvent>(OnDisable);
        }

        protected override void OnStart()
        {
        }

        protected override void OnEnd()
        {
            Unsubscribe<DisableEvent>(OnDisable);
            _instance = null;
        }

        [Command]
        [Alias("Enable")]
        [Description("Enables code/features that were previously disabled. Useful for debugging. Works across networking.")]
        private void EnableCode(string key)
        {
            _enableLookup[key] = true;
            CallEvent(new DisableEvent()
            {
                Key = key,
                IsEnabled = true,
            });
        }

        [Command]
        [Alias("Disable")]
        [Description("Disables code/features. Useful for debugging. Works accross networking")]
        private void DisableCode(string key)
        {
            _enableLookup[key] = false;
            CallEvent(new DisableEvent()
            {
                Key = key,
                IsEnabled = false,
            });
        }

        private void OnDisable(ref DisableEvent evt)
        {
            _enableLookup[evt.Key] = evt.IsEnabled;
        }
    }
}
