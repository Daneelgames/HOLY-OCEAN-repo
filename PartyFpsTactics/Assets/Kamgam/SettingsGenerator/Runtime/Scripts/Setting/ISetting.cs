using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.SettingsGenerator
{
    /// <summary>
    /// Base interface for every setting.
    /// </summary>
    public interface ISetting : ISerializationCallbackReceiver, IQualityChangeReceiver
    {
        public bool HasUserData();
        public void SetHasUserData(bool hasUserData);

        public string GetID();
        public bool MatchesID(string path);

        public SettingData.DataType GetDataType();
        public bool MatchesAnyDataType(IList<SettingData.DataType> dataTypes);

        public List<string> GetGroups();
        public void SetGroups(List<string> groups);
        public bool MatchesAnyGroup(string[] groups);

        public object GetValueAsObject();
        public void SetValueFromObject(object value, bool propagateChange = true);
        public void ResetToDefault();

        public SettingData SerializeValueToData();
        public void DeserializeValueFromData(SettingData data);

        /// <summary>
        /// Called if the value of the settings has changed.
        /// </summary>
        public void OnChanged();

        public void AddPulledFromConnectionListener(System.Action onPulled);
        public void RemovePulledFromConnectionListener(System.Action onPulled);

        /// <summary>
        /// Pushes the value to the connection. Then pulls it.<br />
        /// Clears the changed flag.<br />
        /// Informs all applyListeners of the change.<br />
        /// </summary>
        public void Apply();

        /// <summary>
        /// Returns whether or not this setting has some unapplied changes.<br />
        /// Freshly loaded settings should be marked as "changed".
        /// </summary>
        /// <returns></returns>
        public bool HasUnappliedChanges();

        /// <summary>
        /// Marks the setting as changed.
        /// </summary>
        public void MarkAsChanged();
        public void MarkAsUnchanged();

        /// <summary>
        /// Fetches the default value from the connection. If no user data (saved setting)
        /// was loaded then this will also update the settings current value to the fetched default.
        /// <br /><br />
        /// NOTICE: This does NOT use PullConnection() and therefore does not trigger any listeners.
        /// </summary>
        public void InitializeConnection();
        public bool HasConnection();
        public bool HasConnectionObject();
        public int GetConnectionOrder();
        public void PullFromConnection();
        public void PushToConnection();
        public IConnection GetConnectionInterface();
    }
}
