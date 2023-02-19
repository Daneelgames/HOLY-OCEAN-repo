using UnityEngine;
using UnityEngine.Serialization;

namespace Kamgam.SettingsGenerator
{
    /// <summary>
    /// The Settings Provider creates an instance of the Settings object
    /// and keeps a reference to it so other objects can come and ask
    /// for it at any time.
    /// <br /><br />
    /// It also handles resetting objects before play mode
    /// if Domain-Reload is disabled (via IResetBeforeDomainReload).
    /// </summary>
    [CreateAssetMenu(fileName = "SettingsProvider", menuName = "SettingsGenerator/SettingsProvider", order = 1)]
    public class SettingsProvider : ScriptableObject
#if UNITY_EDITOR
        , IResetBeforeDomainReload
#endif
    {
        [SerializeField, Tooltip("The player prefs key under which your settings will be saved.")]
        protected string playerPrefsKey;

        [Tooltip("The default settings asset.\nYou can leave this empty if you define all your settings via script.")]
        [FormerlySerializedAs("Default")]
        public Settings SettingsAsset;

        protected Settings _settings;
        public Settings Settings
        {
            get
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                    return null;
#endif

                if (_settings == null)
                {
                    // Create a new from the static DefaultSettings (code)
                    if (SettingsAsset == null)
                        _settings = ScriptableObject.CreateInstance<Settings>();
                    else
                        _settings = ScriptableObject.Instantiate(SettingsAsset);

                    // Make a global backup copy of the current quality level (we use this in the Connections to restore it later).
                    QualityPresets.AddCurrentLevel();

                    // Load user settings from storage
                    Settings.LoadFromPlayerPrefs(playerPrefsKey);
                }

                return _settings;
            }
        }


        /// <summary>
        /// Use this to check whether or not the settings have loaded.
        /// </summary>
        public bool HasSettings()
        {
            return _settings != null;
        }

        public void OnEnable()
        {
            if (string.IsNullOrEmpty(playerPrefsKey))
            {
                playerPrefsKey = "SGSettings";
            }
        }

#if UNITY_EDITOR
        // Domain Reload handling
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        protected static void onResetBeforePlayMode()
        {
            DomainReloadUtils.CallOnResetOnAssets(typeof(SettingsProvider));
        }

        public void ResetBeforePlayMode()
        {
            _settings = null;
        }
#endif

        public void Reset()
        {
            if(Settings != null)
                Settings.Reset();
        }

        public void Reset(params string[] ids)
        {
            Settings.Reset(ids);
        }

        public void ResetGroups(params string[] groups)
        {
            Settings.ResetGroups(groups);
        }

        public void ResetGroup(string group)
        {
            Settings.ResetGroups(group);
        }

        public void Apply()
        {
            Settings.Apply();
        }


        // Load & Save

        public void Load()
        {
            if (_settings == null)
            {
                // At the very first load this will be executed.

                // Accessing the "Settings" getter for the very first time
                // causes a load automatically, thus we do not need to load
                // anything here.
                Settings.RefreshRegisteredResolvers();
            }
            else
            {
                // Pull values from connections to initialize the default values.
                Settings.PullFromConnections();

                // Load user settings from storage
                // Also triggers resolver updates (aka Settings.RefreshRegisteredResolvers())
                Settings.LoadFromPlayerPrefs(playerPrefsKey);
            }
        }

        public void ResetToLastSave()
        {
            // Load user settings from storage
            // Also triggers resolver updates (aka Settings.RefreshRegisteredResolvers())
            Settings.LoadFromPlayerPrefs(playerPrefsKey);
        }

        public void Save()
        {
            Settings.SaveToPlayerPrefs(playerPrefsKey);
        }

        public void Delete()
        {
            // The else part exists to support deleting in Editor if not in play mode.
            if (Settings != null)
                Settings.DeleteFromPlayerPrefs(playerPrefsKey);
            else
                Settings.DeletePlayerPrefs(playerPrefsKey);
        }
    }
}
