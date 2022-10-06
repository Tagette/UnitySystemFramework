using UnitySystemFramework.Audio;
using UnitySystemFramework.Core;
using UnitySystemFramework.Menus;
using UnitySystemFramework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UnitySystemFramework.Settings
{
    public class SettingsSystem : BaseSystem
    {
        private class SettingInfo
        {
            public TypeID Type;
            public string Key;
            public string Name;
            public string Description;
            public int Order;
            public object OldValue;
            public object Value;
            public Func<object> Getter;
            public Action<object> Setter;
            public object Min;
            public object Max;
            public Func<object, bool> Validator;
            public Func<object[]> OptionGetter;
            public MemberInfo Member;
            public List<object> Options = new List<object>();
            public List<object> Targets = new List<object>();
            public MenuItem MenuItem;
            public bool ConfirmChange;
            public bool RestartAfterApply;
            public bool ApplyImmediately;
        }

        private class MenuItem
        {
            public MenuItem(string name, GameObject gameObject, MenuSection parent)
            {
                Key = parent?.Key + "/" + name;
                Name = name;
                GameObject = gameObject;
                Parent = parent;
            }

            public string Key;
            public string Name;
            public int Order;
            public GameObject GameObject;
            public MenuSection Parent;
        }

        private class MenuSection : MenuItem
        {
            public MenuSection(string name, GameObject gameObject, MenuSection parent) : base(name, gameObject, parent)
            {
            }

            public Text TitleText;
            public Transform ItemsTransform;
            public readonly List<MenuItem> Items = new List<MenuItem>();
        }

        private class MenuSetting : MenuItem
        {
            public MenuSetting(string name, GameObject gameObject, MenuSection parent) : base(name, gameObject, parent)
            {
            }

            public bool HasSubcribed;
            public Text NameText;
            public Button Button;
            public Toggle Toggle;
            public Slider Slider;
            public Dropdown Dropdown;
        }

        private MenuSystem _menuSystem;
        private PopupSystem _popupSystem;
        private AudioSystem _audioSystem;

        private Menu _settingsMenu;

        private GameObject _sectionPrefab;
        private GameObject _sectionItemPrefab;

        private Button _applyButton;
        private Button _discardButton;
        private Text _discardText;

        private readonly Dictionary<string, SettingInfo> _settings = new Dictionary<string,SettingInfo>();
        private readonly Dictionary<MemberInfo, string> _memberNames = new Dictionary<MemberInfo, string>();
        private readonly HashSet<string> _changedSettings = new HashSet<string>();

        private readonly MenuSection _root = new MenuSection("Root", null, null);
        private bool _confirmChanges;
        private bool _needsRestartToApply;
        private bool _ignoreCallbacks;

        public bool HasChanges => _changedSettings.Count > 0;

        protected override void OnInit()
        {
            _menuSystem = GetSystem<MenuSystem>();
            _popupSystem = GetSystem<PopupSystem>();
            _audioSystem = GetSystem<AudioSystem>();
        }

        protected override void OnStart()
        {
            FindAndAddSettings();

            _settingsMenu = _menuSystem.AddMenu(MenuKey.Settings);
            _sectionPrefab = _settingsMenu.FindFromPath("Templates/Section");
            _sectionItemPrefab = _settingsMenu.FindFromPath("Templates/SettingItem");
            _root.GameObject = _settingsMenu.FindFromPath("Settings/Scroll View/Viewport/Content");
            _root.ItemsTransform = _root.GameObject.transform;
            _applyButton = _settingsMenu.GetElement<Button>("Buttons/Apply");
            _applyButton.AddOnPointerEnter(button => _audioSystem.PlaySound(AudioKey.ButtonHover));
            _applyButton.AddOnClick(button =>
            {
                _audioSystem.PlaySound(AudioKey.ButtonClick);
                ApplySettings();
            });
            _discardButton = _settingsMenu.GetElement<Button>("Buttons/Discard");
            _discardText = _settingsMenu.GetElement<Text>("Buttons/Discard/Text");
            _discardButton.AddOnPointerEnter(button => _audioSystem.PlaySound(AudioKey.ButtonHover));
            _discardButton.AddOnClick(button =>
            {
                _audioSystem.PlaySound(AudioKey.ButtonClick);
                if (HasChanges)
                    DiscardSettings();
                else
                    _menuSystem.CloseMenu(_settingsMenu);
            });

            BuildMenu();
            UpdateButtons();

            AddUpdate(OnUpdate);
        }

        private void OnUpdate()
        {
            TryShowRestartRequired();
            TryShowConfirmation();
        }

        protected override void OnEnd()
        {
            RemoveUpdate(OnUpdate);
            _menuSystem.RemoveMenu(_settingsMenu);
        }

        private void FindAndAddSettings()
        {
            var assemblies = Reflect.GetUserAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    AddTypeSettings(type, null);
                }
            }
        }

        private void UpdateOrder(MenuItem item, bool recurse)
        {
            if (item is MenuSection section)
            {
                section.Items.Sort((a, b) => a.Order.CompareTo(b.Order));

                if (recurse)
                {
                    int index = 0;
                    foreach (var childItem in section.Items)
                    {
                        childItem.GameObject.transform.SetSiblingIndex(index++);
                        UpdateOrder(childItem, recurse);
                    }
                }
            }
        }

        public void BuildMenu()
        {
            foreach (var pair in _settings)
            {
                var setting = pair.Value;
                var key = setting.Key;
                setting.MenuItem = GetOrBuildItem(key, _root);
                UpdateMenuItem(setting);
            }
        }

        private void UpdateMenuItem(SettingInfo setting)
        {
            var key = setting.Key;
            var item = (MenuSetting)setting.MenuItem;
            if (item == null)
                return;
            item.Order = setting.Order;

            _ignoreCallbacks = true;

            bool hasMinMax = setting.Min != null && setting.Max != null;
            item.Slider.gameObject.SetActive(setting.Type == TypeID<float>.ID && hasMinMax);
            if (item.Slider.gameObject.activeSelf) item.Slider.value = (float)setting.Value;
            float lastSound = 0;
            if (!item.HasSubcribed)
            {
                item.Slider.AddOnValueChange(slider =>
                {
                    if (_ignoreCallbacks)
                        return;
                    if (Time.time - lastSound > 0.1f)
                    {
                        lastSound = Time.time;
                        _audioSystem.PlaySound(AudioKey.ButtonHover);
                    }

                    SetSetting(key, item.Slider.value);
                });
            }

            if (hasMinMax)
            {
                item.Slider.minValue = (float)setting.Min;
                item.Slider.maxValue = (float)setting.Max;
                item.Slider.value *= (float)setting.Max;
            }
            item.Button.gameObject.SetActive(false);
            item.Dropdown.gameObject.SetActive(setting.Type.Type.IsEnum || setting.Options.Count > 0 || setting.OptionGetter != null);
            if (item.Dropdown.gameObject.activeSelf)
            {
                item.Dropdown.ClearOptions();
                item.Dropdown.AddOptions(setting.Options.Select(o => o.ToString()).ToList());
                item.Dropdown.value = (int)setting.Value;
                if (!item.HasSubcribed)
                {
                    item.Dropdown.AddOnPointerEnter(dropdown => _audioSystem.PlaySound(AudioKey.ButtonHover));
                    item.Dropdown.AddOnValueChange(dropdown =>
                    {
                        if (_ignoreCallbacks)
                            return;

                        SetSetting(key, item.Dropdown.value);
                    });
                }
            }
            item.Toggle.gameObject.SetActive(setting.Type == TypeID<bool>.ID);
            if (item.Toggle.gameObject.activeSelf) item.Toggle.isOn = (bool)setting.Value;
            if (!item.HasSubcribed)
            {
                item.Toggle.AddOnPointerEnter(dropdown => _audioSystem.PlaySound(AudioKey.ButtonHover));
                item.Toggle.AddOnValueChange(toggle =>
                {
                    if (_ignoreCallbacks)
                        return;

                    SetSetting(key, item.Toggle.isOn);
                });
            }

            item.HasSubcribed = true;
            _ignoreCallbacks = false;
        }

        private MenuSetting GetOrBuildItem(string key, MenuSection parent)
        {
            int sepIndex = key.IndexOf('/');
            if (sepIndex > 0)
            {
                var sectionName = key.Substring(0, sepIndex);

                MenuSection section = (MenuSection)parent.Items.FirstOrDefault(s => s.Name == sectionName);
                if (section == null)
                {
                    var gameObject = Object.Instantiate(_sectionPrefab, Vector3.zero, Quaternion.identity, parent.ItemsTransform);
                    gameObject.SetActive(true);
                    section = new MenuSection(sectionName, gameObject, parent);
                    parent.Items.Add(section);
                    parent.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
                    gameObject.transform.SetSiblingIndex(parent.Items.IndexOf(section));

                    section.TitleText = gameObject.FindComponent<Text>("Title/Text");
                    section.TitleText.text = section.Name;
                    section.ItemsTransform = gameObject.FindFromPath("Items").transform;
                }

                key = key.Substring(sepIndex + 1, key.Length - (sepIndex + 1));
                return GetOrBuildItem(key, section);
            }

            MenuSetting setting = (MenuSetting)parent.Items.FirstOrDefault(s => s.Name == key);
            if (setting == null)
            {
                var gameObject = Object.Instantiate(_sectionItemPrefab, Vector3.zero, Quaternion.identity, parent.ItemsTransform);
                gameObject.SetActive(true);
                setting = new MenuSetting(key, gameObject, parent);

                setting.NameText = gameObject.FindComponent<Text>("Name");
                setting.Button = gameObject.FindComponent<Button>("Button");
                setting.Dropdown = gameObject.FindComponent<Dropdown>("Dropdown");
                setting.Slider = gameObject.FindComponent<Slider>("Slider");
                setting.Toggle = gameObject.FindComponent<Toggle>("Toggle");

                setting.NameText.text = setting.Name;

                parent.Items.Add(setting);
                parent.Items.Sort((a, b) => a.Order.CompareTo(b.Order));
                gameObject.transform.SetSiblingIndex(parent.Items.IndexOf(setting));
            }

            return setting;
        }

        private MenuItem RemoveItem(string key, MenuSection parent)
        {
            int sepIndex = key.IndexOf('/');
            if (sepIndex > 0)
            {
                var sectionName = key.Substring(0, sepIndex);
                key = key.Substring(sepIndex + 1, key.Length - (sepIndex + 1));
                parent = (MenuSection)parent.Items.FirstOrDefault(s => s.Name == sectionName);
                var item = RemoveItem(key, parent) as MenuSection;
                if (item != null && item.Items.Count == 0)
                {
                    item.Parent.Items.Remove(item);
                    Object.Destroy(item.GameObject);
                }
                return item;
            }

            MenuSetting setting = (MenuSetting)parent.Items.FirstOrDefault(s => s.Name == key);
            parent.Items.Remove(setting);
            Object.Destroy(setting.GameObject);
            return setting;
        }

        private void AddTypeSettings(Type type, object target)
        {
            var members = type.GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (var member in members)
            {
                if (!(member is PropertyInfo || member is MethodInfo))
                    continue;

                // A null target is for static settings only.
                if(target == null != member.IsStatic())
                    continue;

                SettingInfo setting = null;
                // Needs to be ordered so the setting attribute is processed first.
                var attributes = member.GetCustomAttributes().OrderByDescending(attribute => attribute is SettingAttribute);
                foreach (var attribute in attributes)
                {
                    if (attribute is SettingAttribute settingAttribute)
                    {
                        if (member is PropertyInfo property)
                        {
                            var getMethod = property.GetGetMethod(true);
                            var setMethod = property.GetSetMethod(true);
                            object Getter() => getMethod.Invoke(target, null);
                            void Setter(object value) => setMethod.Invoke(target, new[] {value});

                            // TODO: Made the name unique to the target or treat them all the same.
                            var key = settingAttribute.Key;
                            if (string.IsNullOrWhiteSpace(key))
                                key = member.Name;
                            var name = key;

                            if (name.Contains("/"))
                            {
                                var split = name.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                                name = split[split.Length - 1];
                            }

                            if (!_settings.TryGetValue(key, out setting))
                                _settings[key] = setting = new SettingInfo();

                            setting.Type = property.PropertyType.GetTypeID();
                            setting.Key = key;
                            setting.Name = name;
                            setting.Description = settingAttribute.Description;
                            setting.Order = settingAttribute.Order;
                            setting.Getter = Getter;
                            setting.Setter = Setter;
                            setting.Member = member;
                            try
                            {
                                setting.Value = Getter();
                                setting.OldValue = setting.Value;
                                if (setting.OptionGetter != null)
                                {
                                    var options = setting.OptionGetter();
                                    setting.Options.Clear();
                                    setting.Options.AddRange(options);
                                }
                                else if (setting.Type.Type.IsEnum)
                                {
                                    setting.Options.Clear();
                                    setting.Options.AddRange(Enum.GetNames(setting.Type.Type));
                                }
                            }
                            catch (Exception ex)
                            {
                                LogException(ex);
                            }

                            _memberNames.Add(member, key);
                        }
                    }
                    else if (attribute is SettingRangeAttribute range)
                    {
                        if (member is PropertyInfo)
                        {
                            // TODO: Made the name unique to the target or treat them all the same.
                            if (_memberNames.TryGetValue(member, out string key))
                                SetSettingRange(key, range.Min, range.Max);
                        }
                    }
                    else if (attribute is OptionsAttribute optionsAttribute)
                    {
                        if (member is MethodInfo method)
                        {
                            if (method.ReturnType != typeof(object[]))
                            {
                                LogError($"The return type of the options getter on '{member.DeclaringType.Name}.{member.Name}()' needs to be of type object[].");
                                continue;
                            }

                            object[] Getter() => (object[]) method.Invoke(target, null);
                            SetOptionGetter(optionsAttribute.Key, Getter);
                        }
                    }
                    else if (attribute is ConfirmAfterApplyAttribute)
                    {
                        if (setting != null)
                            setting.ConfirmChange = true;
                    }
                    else if (attribute is RequiresRestartAfterApplyAttribute)
                    {
                        if (setting != null)
                            setting.RestartAfterApply = true;
                    }
                    else if (attribute is ApplyImmediatelyAttribute)
                    {
                        if (setting != null)
                            setting.ApplyImmediately = true;
                    }
                }
            }
        }

        public void AddSettings(object target)
        {
            var type = target.GetType();
            AddTypeSettings(type, target);
        }

        public void AddSetting(TypeID type, string key, string description, int order, Action<object> setter, Func<object> getter)
        {
            var name = key;

            if (name.Contains("/"))
            {
                var split = name.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                name = split[split.Length - 1];
            }

            if(!_settings.TryGetValue(key, out var setting))
                _settings[key] = setting = new SettingInfo();

            setting.Key = key;
            setting.Name = name;
            setting.Description = description;
            setting.Order = order;
            setting.Setter = setter;
            setting.Getter = getter;
            setting.MenuItem = GetOrBuildItem(key, _root);

            try
            {
                setting.Value = getter();
                setting.OldValue = setting.Value;
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        public void RemoveSetting(string key)
        {
            if(_settings.TryGetValue(key, out var setting))
            {
                _memberNames.Remove(setting.Member);
                _settings.Remove(key);
                RemoveItem(setting.Key, setting.MenuItem.Parent); // There is always a parent.
            }
        }

        public void SetSettingRange(string key, object min, object max)
        {
            if (_settings.TryGetValue(key, out var setting))
            {
                setting.Min = min;
                setting.Max = max;

                UpdateMenuItem(setting);
            }
        }

        public void AddSettingValidator(string key, Func<object, bool> validator)
        {
            if (_settings.TryGetValue(key, out var setting))
            {
                setting.Validator += validator;
            }
        }

        public void RemoveSettingValidator(string key, Func<object, bool> validator)
        {
            if (_settings.TryGetValue(key, out var setting))
            {
                setting.Validator -= validator;
            }
        }

        public void SetOptionGetter(string key, Func<object[]> getter)
        {
            if (!_settings.TryGetValue(key, out var setting))
                _settings[key] = setting = new SettingInfo();

            setting.OptionGetter = getter;
            try
            {
                var options = setting.OptionGetter();
                setting.Options.Clear();
                setting.Options.AddRange(options);
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        public void RemoveOptionGetter(string key)
        {
            if (_settings.TryGetValue(key, out var setting))
            {
                setting.OptionGetter = null;
            }
        }

        public void AddSettingOption(string key, object value)
        {
            if (_settings.TryGetValue(key, out var setting))
            {
                setting.Options.Add(value);
            }
        }

        public void RemoveSettingOption(string key, object value)
        {
            if (_settings.TryGetValue(key, out var setting))
            {
                for (int i = setting.Options.Count - 1; i >= 0; i--)
                {
                    if (Equals(setting.Options[i], value))
                    {
                        setting.Options.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public object[] GetSettingOptions(string key)
        {
            if (_settings.TryGetValue(key, out var setting))
                return setting.Options.ToArray();

            return Array.Empty<object>();
        }

        public object GetSetting(string key)
        {
            if (_settings.TryGetValue(key, out var setting))
                return setting.Value;

            return default;
        }

        public void SetSetting(string key, object value)
        {
            if (_settings.TryGetValue(key, out var setting))
            {
                setting.OldValue = setting.Value;
                setting.Value = value;
                if (setting.ApplyImmediately)
                {
                    if(setting.RestartAfterApply)
                        _needsRestartToApply = true;

                    ApplySetting(setting);
                }
                else
                    _changedSettings.Add(setting.Key);

                UpdateButtons();
            }
        }

        public void ApplySettings()
        {
            foreach (var pair in _settings)
            {
                var setting = pair.Value;

                if(setting.RestartAfterApply)
                    _needsRestartToApply = true;

                if (setting.ConfirmChange)
                    _confirmChanges = true;

                ApplySetting(setting);
            }

            _changedSettings.Clear();
            UpdateButtons();
        }

        public void ApplySetting(string key)
        {
            if (_settings.TryGetValue(key, out var setting))
            {
                if (setting.RestartAfterApply)
                    _needsRestartToApply = true;

                if (setting.ConfirmChange)
                    _confirmChanges = true;

                ApplySetting(setting);
            }
        }

        private void TryShowRestartRequired()
        {
            if (_needsRestartToApply)
            {
                _needsRestartToApply = false;
                _popupSystem.ShowPopup(
                    "Restart Required",
                    "The changed settings require a restart of the game in order to take effect.",
                    new[] { "Ok" },
                    isBlocking: true);
            }
        }

        private void TryShowConfirmation()
        {
            if (_confirmChanges)
            {
                _confirmChanges = false;
                _popupSystem.ShowPopup(
                    "Confirm Changes",
                    "Are you sure you want to make these changes?",
                    new[] { "Keep Changes", "Revert" },
                    expireSeconds: 30,
                    isBlocking: true,
                    i =>
                    {
                        if (i != 0)
                            RevertSettings();
                    });
            }
        }

        private void ApplySetting(SettingInfo setting)
        {
            bool isValid = true;
            if (setting.Validator != null)
            {
                try
                {
                    isValid = setting.Validator(setting.Value);
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }

            if (isValid)
            {
                try
                {
                    setting.Setter(setting.Value);
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }
            }

            CallEvent(new SettingApplyEvent()
            {
                Key = setting.Key,
                Type = setting.Type,
                Description = setting.Description,
                OldValue = setting.OldValue,
                Value = setting.Value,
            });
        }

        public void SetAndApplySetting(string key, object value)
        {
            SetSetting(key, value);
            ApplySetting(key);
        }

        public void UpdateSetting(string key)
        {
            if (_settings.TryGetValue(key, out var setting))
            {
                setting.OldValue = setting.Value;

                try
                {
                    setting.Value = setting.Getter();

                    if (setting.OptionGetter != null)
                    {
                        var options = setting.OptionGetter();
                        setting.Options.Clear();
                        setting.Options.AddRange(options);
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }

                _changedSettings.Remove(setting.Key);
                UpdateMenuItem(setting);
                UpdateButtons();
            }
        }

        public void UpdateSettings()
        {
            foreach (var pair in _settings)
            {
                var setting = pair.Value;

                setting.OldValue = setting.Value;

                try
                {
                    setting.Value = setting.Getter();
                    if (!setting.ApplyImmediately && !Equals(setting.OldValue, setting.Value))
                        _changedSettings.Add(setting.Key);

                    if (setting.ApplyImmediately)
                        ApplySetting(setting);
                }
                catch (Exception ex)
                {
                    LogException(ex);
                }

                UpdateMenuItem(setting);
            }

            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (HasChanges)
            {
                _applyButton.gameObject.SetActive(true);
                _discardText.text = "Discard";
            }
            else
            {
                _applyButton.gameObject.SetActive(false);
                _discardText.text = "Back";
            }
        }

        public void RevertSettings()
        {
            foreach (var pair in _settings)
            {
                var setting = pair.Value;

                setting.Value = setting.OldValue;
                var item = (MenuSetting) setting.MenuItem;
                _ignoreCallbacks = true;
                if (item.Slider.gameObject.activeSelf) item.Slider.value = (float)setting.Value;
                if (item.Dropdown.gameObject.activeSelf) item.Dropdown.value = (int)setting.Value;
                if (item.Toggle.gameObject.activeSelf) item.Toggle.isOn = (bool)setting.Value;
                _ignoreCallbacks = false;
                ApplySetting(setting);
            }

            _changedSettings.Clear();
            UpdateButtons();
        }

        public void DiscardSetting(string key)
        {
            if (_changedSettings.Contains(key) && _settings.TryGetValue(key, out var setting))
            {
                _changedSettings.Remove(setting.Key);
                setting.Value = setting.OldValue;
                UpdateButtons();
            }
        }

        public void DiscardSettings()
        {
            foreach (var key in _changedSettings)
            {
                if(!_settings.TryGetValue(key, out var setting))
                    continue;

                setting.Value = setting.OldValue;
                var item = (MenuSetting)setting.MenuItem;
                _ignoreCallbacks = true;
                if (item.Slider.gameObject.activeSelf) item.Slider.value = (float)setting.Value;
                if (item.Dropdown.gameObject.activeSelf) item.Dropdown.value = (int)setting.Value;
                if (item.Toggle.gameObject.activeSelf) item.Toggle.isOn = (bool)setting.Value;
                _ignoreCallbacks = false;
            }
            _changedSettings.Clear();
            UpdateButtons();
        }
    }
}
