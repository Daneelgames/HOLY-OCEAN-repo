using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kamgam.SettingsGenerator
{
    /// <summary>
    /// Settings are a scriptable object and they are loaded
    /// immediately (synchronously) upon first access through
    /// the SettingsProvider.
    /// 
    /// If a new setting is added via code (GetOrCreate(..))
    /// then Apply() should be called on it to push the default
    /// value to the connection.
    /// </summary>
    [CreateAssetMenu(fileName = "Settings", menuName = "SettingsGenerator/Settings", order = 2)]
    public partial class Settings : ScriptableObject, ISerializationCallbackReceiver
    {
        protected List<ISetting> _settingsCache = new List<ISetting>();

        [SerializeField]
        protected List<SettingBool> _bools = new List<SettingBool>();

        [SerializeField]
        protected List<SettingOption> _options = new List<SettingOption>();

        [SerializeField]
        protected List<SettingInt> _integers = new List<SettingInt>();

        [SerializeField]
        protected List<SettingFloat> _floats = new List<SettingFloat>();

        [SerializeField]
        protected List<SettingString> _strings = new List<SettingString>();

        [SerializeField]
        protected List<SettingColor> _colors = new List<SettingColor>();

        [SerializeField]
        protected List<SettingColorOption> _colorOptions = new List<SettingColorOption>();

        [SerializeField]
        protected List<SettingKeyCombination> _keyCombinations = new List<SettingKeyCombination>();

        public void RebuildSettingsCache()
        {
            _settingsCache.Clear();

            foreach (var setting in _bools)
                if(setting != null)
                    _settingsCache.Add(setting); 

            foreach (var setting in _options)
                if (setting != null)
                    _settingsCache.Add(setting);

            foreach (var setting in _integers)
                if (setting != null)
                    _settingsCache.Add(setting);

            foreach (var setting in _floats)
                if (setting != null)
                    _settingsCache.Add(setting);

            foreach (var setting in _strings)
                if (setting != null)
                    _settingsCache.Add(setting);

            foreach (var setting in _colors)
                if (setting != null)
                    _settingsCache.Add(setting);

            foreach (var setting in _colorOptions)
                if (setting != null)
                    _settingsCache.Add(setting);

            foreach (var setting in _keyCombinations)
                if (setting != null)
                    _settingsCache.Add(setting);
        }

        public List<ISetting> GetAllSettings()
        {
            return _settingsCache;
        }

        public void RemoveSetting(ISetting setting)
        {
            RemoveSetting(setting.GetID());
        }

        public void RemoveSetting(string id)
        {
            removeSetting(_bools, id);
            removeSetting(_options, id);
            removeSetting(_integers, id);
            removeSetting(_floats, id);
            removeSetting(_strings, id);
            removeSetting(_colors, id);
            removeSetting(_colorOptions, id);
            removeSetting(_keyCombinations, id);

            removeSetting(_settingsCache, id);
        }

        protected void removeSetting<T>(List<T> list, string id) where T : ISetting
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].MatchesID(id))
                {
                    list.RemoveAt(i);
                    break;
                }
            }
        }

        public void OnBeforeSerialize()
        {
            RebuildSettingsCache();

            foreach (var setting in _settingsCache)
            {
                setting.OnBeforeSerialize();
            }
        }

        public void OnAfterDeserialize()
        {
            RebuildSettingsCache();

            foreach (var setting in _settingsCache)
            {
                setting.OnAfterDeserialize();
            }
        }

        public void LoadFromPlayerPrefs(string playerPrefsKey)
        {
            var json = PlayerPrefs.GetString(playerPrefsKey, null);
            if (!string.IsNullOrEmpty(json))
            {
                fromJson(json);
            }

            postLoad();
        }

        /// <summary>
        /// Initialized connections, marks all settings as changed and the applied all settings.
        /// <br /><br />
        /// It is important that this is called BEFORE the first Connection.SET() call on any setting
        /// because this fetches the default values from the connections.
        /// </summary>
        protected void postLoad()
        {
            RebuildSettingsCache();

            // Pull the intial default values from connections (only if no user setting was saved).
            // This also fetches the default value from the connection.
            foreach (var setting in _settingsCache)
            {
                setting.InitializeConnection(); // auto initializes default value for settings with connections

                // Explicity initialize value with default if settings does not yet have user data.
                if (!setting.HasConnection() && !setting.HasUserData())
                {
                    setting.ResetToDefault();
                }
            }

            // Mark all as changed (important for Connections which effect many other setting, example: QualityConnection).
            foreach (var setting in _settingsCache)
            {
                setting.MarkAsChanged();
            }

            // Apply the settings after loading them (that's what most people would expect).
            Apply(changedOnly: true);

            // Initially this does nothing in the current setup as all resolvers
            // use the provider which loads the settings synchronously and
            // thus no resolver can be registered at this time.
            // However it does refresh if it's called after the initial load.
            RefreshRegisteredResolvers();
        }

        public void SaveToPlayerPrefs(string playerPrefsKey)
        {
            var json = toJson();
            if (!string.IsNullOrEmpty(json))
            {
                PlayerPrefs.SetString(playerPrefsKey, json);
                PlayerPrefs.Save();
            }
        }

        public void DeleteFromPlayerPrefs(string playerPrefsKey)
        {
            DeletePlayerPrefs(playerPrefsKey);
        }

        public static void DeletePlayerPrefs(string playerPrefsKey)
        {
            PlayerPrefs.DeleteKey(playerPrefsKey);
            PlayerPrefs.Save();
        }

        protected string toJson()
        {
            return SettingsSerializer.ToJson(this);
        }

        protected void fromJson(string json)
        {
            SettingsSerializer.FromJson(json, this);
        }

        /// <summary>
        /// Applies the settings.<br />
        /// This means that if a setting has a connection it will be pushed and then pulled.
        /// </summary>
        /// <param name="changedOnly">Apply only those which have changed.</param>
        public void Apply(bool changedOnly = true)
        {
            var sortedSettings = getSettingsOrderedByConnectionOrderASC(_settingsCache);
            ISetting setting;

            // If all settings should be applied then mark them all as changed first.
            // We do this so that even if a setting implements Apply() as being executed only
            // if changed it will still execute.
            if (!changedOnly)
            {
                for (int i = 0; i < sortedSettings.Count; i++)
                {
                    setting = sortedSettings[i];
                    setting.MarkAsChanged();
                }
            }

            for (int i = 0; i < sortedSettings.Count; i++)
            {
                setting = sortedSettings[i];
                if (changedOnly && !setting.HasUnappliedChanges())
                    continue;

                setting.Apply();
            }
        }

        /// <summary>
        /// Pulls all values from the connections.
        /// </summary>
        /// <param name="exceptUnapplied">Set to TRUE if you do not want values to be pulled for settings which still have unapplied changes.</param>
        public void PullFromConnections(bool exceptUnapplied = false)
        {
            var sortedSettings = getSettingsOrderedByConnectionOrderASC(_settingsCache);
            ISetting setting;
            for (int i = 0; i < sortedSettings.Count; i++)
            {
                setting = sortedSettings[i];
                if (!setting.HasConnection())
                    continue;

                if (exceptUnapplied && setting.HasUnappliedChanges())
                    continue;
                
                setting.PullFromConnection();
            }
        }

        public void PushToConnections()
        {
            var sortedSettings = getSettingsOrderedByConnectionOrderASC(_settingsCache);
            ISetting setting;
            for (int i = 0; i < sortedSettings.Count; i++)
            {
                setting = sortedSettings[i];
                if (setting.HasConnection())
                {
                    setting.PushToConnection();
                }
            }
        }

        public void PushToConnections(params string[] groups)
        {
            var sortedSettings = getSettingsOrderedByConnectionOrderASC(_settingsCache);
            ISetting setting;
            for (int i = 0; i < sortedSettings.Count; i++)
            {
                setting = sortedSettings[i];
                if (setting.MatchesAnyGroup(groups))
                {
                    if (setting.HasConnection())
                    {
                        setting.PushToConnection();
                    }
                }
            }
        }

#region sort by connection order
        protected List<ISetting> _tmpSettingsSortedByConnectionOrder;

        protected List<ISetting> getSettingsOrderedByConnectionOrderASC(IEnumerable<ISetting> settings)
        {
            if (_tmpSettingsSortedByConnectionOrder == null)
                _tmpSettingsSortedByConnectionOrder = new List<ISetting>();
            _tmpSettingsSortedByConnectionOrder.Clear();

            foreach (var setting in settings)
            {
                _tmpSettingsSortedByConnectionOrder.Add(setting);
            }

            _tmpSettingsSortedByConnectionOrder.Sort(compartByConnectionOrder);

            return _tmpSettingsSortedByConnectionOrder;
        }

        protected int compartByConnectionOrder(ISetting a, ISetting b)
        {
            return a.GetConnectionOrder() - b.GetConnectionOrder();
        }
#endregion

#region sort by name/id
        protected List<ISetting> _tmpSettingsSortedByName;

        protected List<ISetting> getSettingsOrderedByID(IEnumerable<ISetting> settings)
        {
            if (_tmpSettingsSortedByName == null)
                _tmpSettingsSortedByName = new List<ISetting>();
            _tmpSettingsSortedByName.Clear();

            foreach (var setting in settings)
            {
                _tmpSettingsSortedByName.Add(setting);
            }

            _tmpSettingsSortedByName.Sort(compareByID);

            return _tmpSettingsSortedByName;
        }

        protected int compareByID(ISetting a, ISetting b)
        {
            return string.Compare(a.GetID(), b.GetID());
        }
        #endregion

        public bool HasID(string id)
        {
            return GetSetting(id) != null;
        }

        public ISetting GetSetting(string id)
        {
            foreach (var setting in _settingsCache)
            {
                if (setting.GetID() == id)
                {
                    return setting;
                }
            }

            return null;
        }

        protected bool doesOtherSettingExist(string id, SettingData.DataType dataType)
        {
            var setting = GetSetting(id);
            if (setting != null && setting.GetDataType() != dataType)
            {
                Debug.LogError("You are trying to create '"+id+"' (type: '"+dataType+"') but another '"+id+"' with a DIFFERENT type ('" + setting.GetDataType() + "') already exists. Aborting creation. Duplicate IDs are not allowed.");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Search for the id in the settings.
        /// <br /><br />
        /// If a setting with that id is found then it will
        /// override the connection and groups of that setting and return it.<br />
        /// The "defaultValue" will be ignored if the settings already exists
        /// or if a connection is specified.
        /// <br /><br />
        /// If no setting is found then a new setting with the id,
        /// defaultValue, groups and connection will be created.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="defaultValue">Default value if no connection is set.</param>
        /// <param name="groups"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public SettingBool GetOrCreateBool(string id, bool defaultValue = false, List<string> groups = null, IConnection<bool> connection = null)
        {
            // Try to find
            var setting = GetBool(id);

            // If not found then create
            if (setting == null)
            {
                setting = addBool(id, defaultValue, groups);
            }
            else
            {
                if (groups != null)
                {
                    setting.SetGroups(groups);
                }
            }

            // Override connection (if one was given)
            if (connection != null)
            {
                setting.SetConnection(connection);
            }

            return setting;
        }

        public SettingBool GetBool(string id)
        {
            foreach (var setting in _bools)
            {
                if (setting.GetID() == id)
                {
                    return setting;
                }
            }

            return null;
        }

        protected SettingBool addBool(string id, bool value, List<string> groups = null)
        {
            if (doesOtherSettingExist(id, SettingData.DataType.Bool))
                return null;

            var setting = new SettingBool(id, value, groups);
            _bools.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        public SettingBool AddBoolFromSerializedData(SettingData data, List<string> groups = null)
        {
            if (doesOtherSettingExist(data.ID, SettingData.DataType.Bool))
                return null;

            var setting = new SettingBool(data, groups);
            _bools.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        /// <summary>
        /// Search for the id in the settings.
        /// <br /><br />
        /// If a setting with that id is found then it will
        /// override the connection and groups of that setting and return it.<br />
        /// The "defaultValue" will be ignored if the settings already exists
        /// or if a connection is specified.
        /// <br /><br />
        /// If no setting is found then a new setting with the id,
        /// defaultValue, groups and connection will be created.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="defaultValue">Default value if no connection is set.</param>
        /// <param name="groups"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public SettingColor GetOrCreateColor(string id, Color defaultValue, List<string> groups = null, IConnection<Color> connection = null)
        {
            var setting = GetColor(id);
            if (setting == null)
            {
                setting = addColor(id, defaultValue, groups);
            }
            else
            {
                if (groups != null)
                {
                    setting.SetGroups(groups);
                }
            }

            if (connection != null)
            {
                setting.SetConnection(connection);
            }
            return setting;
        }

        public SettingColor GetColor(string id)
        {
            foreach (var setting in _colors)
            {
                if (setting.GetID() == id)
                {
                    return setting;
                }
            }

            return null;
        }

        protected SettingColor addColor(string id, Color value, List<string> groups = null)
        {
            if (doesOtherSettingExist(id, SettingData.DataType.Color))
                return null;

            var setting = new SettingColor(id, value, groups);
            _colors.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        public SettingColor AddColorFromSerializedData(SettingData data, List<string> groups = null)
        {
            if (doesOtherSettingExist(data.ID, SettingData.DataType.Color))
                return null;

            var setting = new SettingColor(data, groups);
            _colors.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        /// <summary>
        /// Search for the id in the settings.
        /// <br /><br />
        /// If a setting with that id is found then it will
        /// override the connection, options and groups of that setting and return it.<br />
        /// The "defaultValue" will be ignored if the settings already exists
        /// or if a connection is specified.
        /// <br /><br />
        /// If no setting is found then a new setting with the id,
        /// defaultValue, groups and connection will be created.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="defaultOption">Default value if no connection is set.</param>
        /// <param name="groups"></param>
        /// <param name="options"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public SettingColorOption GetOrCreateColorOption(string id, int defaultOption = 0, List<string> groups = null, List<Color> options = null, IConnectionWithOptions<Color> connection = null)
        {
            var setting = GetColorOption(id);
            if (setting == null)
            {
                setting = addColorOption(id, defaultOption, groups, options);
            }
            else
            {
                if (groups != null && groups.Count > 0)
                {
                    setting.SetGroups(groups);
                }

                if (options != null && options.Count > 0)
                {
                    setting.SetOptionLabels(options);
                    RefreshRegisteredResolvers(id);
                }
            }

            if (connection != null)
            {
                setting.SetConnection(connection);
            }

            return setting;
        }

        public SettingColorOption GetColorOption(string id)
        {
            foreach (var setting in _colorOptions)
            {
                if (setting.GetID() == id)
                {
                    return setting;
                }
            }

            return null;
        }

        protected SettingColorOption addColorOption(string id, int selectedIndex, List<string> groups = null, List<Color> options = null)
        {
            if (doesOtherSettingExist(id, SettingData.DataType.ColorOption))
                return null;

            var setting = new SettingColorOption(id, selectedIndex, groups, options);
            _colorOptions.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        public SettingColorOption AddColorOptionFromSerializedData(SettingData data, List<string> groups = null, List<Color> options = null)
        {
            if (doesOtherSettingExist(data.ID, SettingData.DataType.ColorOption))
                return null;

            var setting = new SettingColorOption(data, groups, options);
            _colorOptions.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        /// <summary>
        /// Search for the id in the settings.
        /// <br /><br />
        /// If a setting with that id is found then it will
        /// override the connection and groups of that setting and return it.<br />
        /// The "defaultValue" will be ignored if the settings already exists
        /// or if a connection is specified.
        /// <br /><br />
        /// If no setting is found then a new setting with the id,
        /// defaultValue, groups and connection will be created.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="defaultValue">Default value if no connection is set.</param>
        /// <param name="groups"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public SettingFloat GetOrCreateFloat(string id, float defaultValue = 0f, List<string> groups = null, IConnection<float> connection = null)
        {
            var setting = GetFloat(id);
            if (setting == null)
            {
                setting = addFloat(id, defaultValue, groups);
            }
            else
            {
                if (groups != null)
                {
                    setting.SetGroups(groups);
                }
            }

            if (connection != null)
            {
                setting.SetConnection(connection);
            }
            return setting;
        }

        public SettingFloat GetFloat(string id)
        {
            foreach (var setting in _floats)
            {
                if (setting.GetID() == id)
                {
                    return setting;
                }
            }

            return null;
        }

        protected SettingFloat addFloat(string id, float value, List<string> groups = null)
        {
            if (doesOtherSettingExist(id, SettingData.DataType.Float))
                return null;

            var setting = new SettingFloat(id, value, groups);
            _floats.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        public SettingFloat AddFloatFromSerializedData(SettingData data, List<string> groups = null)
        {
            if (doesOtherSettingExist(data.ID, SettingData.DataType.Float))
                return null;

            var setting = new SettingFloat(data, groups);
            _floats.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        /// <summary>
        /// Search for the id in the settings.
        /// <br /><br />
        /// If a setting with that id is found then it will
        /// override the connection and groups of that setting and return it.<br />
        /// The "defaultValue" will be ignored if the settings already exists
        /// or if a connection is specified.
        /// <br /><br />
        /// If no setting is found then a new setting with the id,
        /// defaultValue, groups and connection will be created.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="defaultValue">Default value if no connection is set.</param>
        /// <param name="groups"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public SettingInt GetOrCreateInt(string id, int defaultValue = 0, List<string> groups = null, IConnection<int> connection = null)
        {
            var setting = GetInt(id);
            if (setting == null)
            {
                setting = addInt(id, defaultValue, groups);
            }
            else
            {
                if (groups != null)
                {
                    setting.SetGroups(groups);
                }
            }

            if (connection != null)
            {
                setting.SetConnection(connection);
            }
            return setting;
        }

        public SettingInt GetInt(string id)
        {
            foreach (var setting in _integers)
            {
                if (setting.GetID() == id)
                {
                    return setting;
                }
            }

            return null;
        }

        protected SettingInt addInt(string id, int value, List<string> groups = null)
        {
            if (doesOtherSettingExist(id, SettingData.DataType.Int))
                return null;

            var setting = new SettingInt(id, value, groups);
            _integers.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        public SettingInt AddIntFromSerializedData(SettingData data, List<string> groups = null)
        {
            if (doesOtherSettingExist(data.ID, SettingData.DataType.Int))
                return null;

            var setting = new SettingInt(data, groups);
            _integers.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        /// <summary>
        /// Search for the id in the settings.
        /// <br /><br />
        /// If a setting with that id is found then it will
        /// override the connection and groups of that setting and return it.<br />
        /// The "defaultValue" will be ignored if the settings already exists
        /// or if a connection is specified.
        /// <br /><br />
        /// If no setting is found then a new setting with the id,
        /// defaultValue, groups and connection will be created.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="defaultValue">Default value if no connection is set.</param>
        /// <param name="groups"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public SettingKeyCombination GetOrCreateKeyCombination(string id, KeyCombination defaultValue, List<string> groups = null, IConnection<KeyCombination> connection = null)
        {
            var setting = GetKeyCombination(id);
            if (setting == null)
            {
                setting = addKeyCombination(id, defaultValue, groups);
            }
            else
            {
                if (groups != null)
                {
                    setting.SetGroups(groups);
                }
            }

            if (connection != null)
            {
                setting.SetConnection(connection);
            }
            return setting;
        }

        protected SettingKeyCombination addKeyCombination(string id, KeyCombination value, List<string> groups = null)
        {
            if (doesOtherSettingExist(id, SettingData.DataType.KeyCombination))
                return null;

            var setting = new SettingKeyCombination(id, value, groups);
            _keyCombinations.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        public SettingKeyCombination AddKeyCombinationFromSerializedData(SettingData data, List<string> groups = null)
        {
            if (doesOtherSettingExist(data.ID, SettingData.DataType.KeyCombination))
                return null;

            var setting = new SettingKeyCombination(data, groups);
            _keyCombinations.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        public SettingKeyCombination GetKeyCombination(string id)
        {
            foreach (var setting in _keyCombinations)
            {
                if (setting.GetID() == id)
                {
                    return setting;
                }
            }

            return null;
        }

        /// <summary>
        /// Search for the id in the settings.
        /// <br /><br />
        /// If a setting with that id is found then it will
        /// override the connection, options and groups of that setting and return it.<br />
        /// The "defaultValue" will be ignored if the settings already exists
        /// or if a connection is specified.
        /// <br /><br />
        /// If no setting is found then a new setting with the id,
        /// defaultValue, groups and connection will be created.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="defaultOption">Default value if no connection is set.</param>
        /// <param name="groups"></param>
        /// <param name="options"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public SettingOption GetOrCreateOption(string id, int defaultOption = 0, List<string> groups = null, List<string> options = null, IConnectionWithOptions<string> connection = null)
        {
            var setting = GetOption(id);
            if (setting == null)
            {
                setting = addOption(id, defaultOption, groups, options);
            }
            else
            {
                if (groups != null && groups.Count > 0)
                {
                    setting.SetGroups(groups);
                }

                if (options != null && options.Count > 0)
                {
                    setting.SetOptionLabels(options);
                    RefreshRegisteredResolvers(id);
                }
            }

            if (connection != null)
            {
                setting.SetConnection(connection);
            }

            return setting;
        }

        public SettingOption GetOption(string id)
        {
            foreach (var setting in _options)
            {
                if (setting.GetID() == id)
                {
                    return setting;
                }
            }

            return null;
        }

        protected SettingOption addOption(string id, int selectedIndex, List<string> groups = null, List<string> options = null)
        {
            if (doesOtherSettingExist(id, SettingData.DataType.Option))
                return null;

            var setting = new SettingOption(id, selectedIndex, groups, options);
            _options.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        public SettingOption AddOptionFromSerializedData(SettingData data, List<string> groups = null, List<string> options = null)
        {
            if (doesOtherSettingExist(data.ID, SettingData.DataType.Option))
                return null;

            var setting = new SettingOption(data, groups, options);
            _options.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        /// <summary>
        /// Search for the id in the settings.
        /// <br /><br />
        /// If a setting with that id is found then it will
        /// override the connection and groups of that setting and return it.<br />
        /// The "defaultValue" will be ignored if the settings already exists
        /// or if a connection is specified.
        /// <br /><br />
        /// If no setting is found then a new setting with the id,
        /// defaultValue, groups and connection will be created.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="defaultValue">Default value if no connection is set.</param>
        /// <param name="groups"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public SettingString GetOrCreateString(string id, string defaultValue = "", List<string> groups = null, IConnection<string> connection = null)
        {
            var setting = GetString(id);
            if (setting == null)
            {
                setting = addString(id, defaultValue, groups);
            }
            else
            {
                if (groups != null)
                {
                    setting.SetGroups(groups);
                }
            }

            if (connection != null)
            {
                setting.SetConnection(connection);
            }
            return setting;
        }

        public SettingString GetString(string id)
        {
            foreach (var setting in _strings)
            {
                if (setting.GetID() == id)
                {
                    return setting;
                }
            }

            return null;
        }

        protected SettingString addString(string id, string value, List<string> groups = null)
        {
            if (doesOtherSettingExist(id, SettingData.DataType.String))
                return null;

            var setting = new SettingString(id, value, groups);
            _strings.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        public SettingString AddStringFromSerializedData(SettingData data, List<string> groups = null)
        {
            if (doesOtherSettingExist(data.ID, SettingData.DataType.String))
                return null;

            var setting = new SettingString(data, groups);
            _strings.Add(setting);
            RebuildSettingsCache();
            return setting;
        }

        public object GetValue(string id)
        {
            var setting = GetSetting(id);
            if (setting != null)
                return setting.GetValueAsObject();
            else
                return null;
        }

        public T GetValue<T>(string id)
        {
            var val = GetValue(id);
            if (val != null)
            {
                if (val is T)
                {
                    return (T)val;
                }
                else
                {
                    Debug.LogError(
                        "SGSettings: The value for id '" + id + "' could not be read because of a type mismatch.\n" +
                        "The type you requested (" + typeof(T).Name.Replace("Single", "Float") + ") does not match " +
                        "the '" + id + "' field in Settings (" + val.GetType().Name.Replace("Single", "Float") + ").\n" +
                        "You may also get an ArgumentException if you try to set this value."
                        );
                    return default(T);
                }
            }

            return default(T);
        }

        public void SetValue(string id, object value)
        {
            var setting = GetSetting(id);
            if (setting != null)
            {
                setting.SetValueFromObject(value);
            }
        }

        /// <summary>
        /// Use this to notify settings about a change in QualitySettings.<br />
        /// The default "QualityConnection" implementation uses this.
        /// </summary>
        /// <param name="qualityLevel"></param>
        /// <param name="excludeChanged"></param>
        public void OnQualityChanged(int qualityLevel, bool excludeChanged = false)
        {
            var sortedSettings = getSettingsOrderedByConnectionOrderASC(_settingsCache);
            ISetting setting;
            for (int i = 0; i < sortedSettings.Count; i++)
            {
                setting = sortedSettings[i];
                if (excludeChanged && setting.HasUnappliedChanges())
                    continue;

                setting.OnQualityChanged(qualityLevel);
            }
        }

        public string[] GetSettingIDsOrderedByName(bool filterByDataType = false, params SettingData.DataType[] dataTypes)
        {
            var settings = getSettingsOrderedByID(_settingsCache);
            var result = settings
                .Where(s => !filterByDataType || dataTypes.Contains(s.GetDataType()))
                .Select(s => s.GetID());
            return result.ToArray();
        }
    }
}
