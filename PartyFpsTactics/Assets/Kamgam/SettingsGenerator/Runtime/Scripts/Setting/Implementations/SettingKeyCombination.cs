using Kamgam.UGUIComponentsForSettings;
using System.Collections.Generic;
using UnityEngine;
namespace Kamgam.SettingsGenerator
{
    [System.Serializable]
    public class SettingKeyCombination : SettingWithValue<KeyCombination>, ISettingWithConnection<KeyCombination>
    {
        public const SettingData.DataType DataType = SettingData.DataType.KeyCombination;

        [System.NonSerialized]
        public IConnection<KeyCombination> Connection;

        [SerializeField]
        ConnectionSO ConnectionObject;

        [System.NonSerialized]
        protected KeyCombination _keyCombination;

        public override KeyCombination GetValue()
        {
            return _keyCombination;
        }

        protected bool _valueInitialized;

        public override void SetValue(KeyCombination keyCombination, bool propagateChange = true)
        {
            if (_keyCombination.Equals(keyCombination) && _valueInitialized)
                return;
            _valueInitialized = true;

            _keyCombination = keyCombination;

            if (propagateChange)
            {
                OnChanged();

                if (ApplyImmediately)
                    PushToConnection();
            }
        }

        public SettingKeyCombination(SettingData data, List<string> groups = null) : base(data, groups) { }

        public SettingKeyCombination(string path, KeyCombination keyCombination, List<string> groups = null) : base(path, groups)
        {
            _defaultValue = keyCombination;
            SetValue(keyCombination);
        }

        public override void ResetToDefault()
        {
            SetValue(_defaultValue);

            if (HasConnection() && ApplyImmediately)
                PushToConnection();
        }

        public override object GetValueAsObject()
        {
            return GetValue();
        }

        public override void SetValueFromObject(object value, bool propagateChange = true)
        {
            SetValue((KeyCombination)value, propagateChange);
        }

        public override SettingData.DataType GetDataType()
        {
            return DataType;
        }

        public override SettingData SerializeValueToData()
        {
            var data = new SettingData(ID, DataType);

            // serialize into primitives
            var keyCombo = GetValue();
            data.IntValues = new int[] { (int)keyCombo.Key, (int)keyCombo.ModifierKey };

            return data;
        }

        public override void DeserializeValueFromData(SettingData data)
        {
            if (!checkDataType(data.Type, DataType))
                return;

            // deserialize from primitives
            var keyCombo = new KeyCombination((UniversalKeyCode)data.IntValues[0], (UniversalKeyCode)data.IntValues[1]);
            SetValue(keyCombo, propagateChange: false);
        }

        public override bool HasConnection()
        {
            return Connection != null;
        }

        public override bool HasConnectionObject()
        {
            return ConnectionObject != null;
        }

        public override void PullFromConnection()
        {
            if (HasConnection())
            {
                SetValue(Connection.Get(), propagateChange: false);
                invokePulledFromConnectionListeners();
            }
        }

        public override void PushToConnection()
        {
            base.PushToConnection();

            if (HasConnection())
                Connection.Set(GetValue());
        }

        public override int GetConnectionOrder()
        {
            if (HasConnection())
                return Connection.GetOrder();
            else
                return ConnectionDefaults.Order;
        }

        public void SetConnection(IConnection<KeyCombination> connection)
        {
            Connection = connection;

            if (connection != null)
            {
                InitializeConnection();
            }
        }

        public IConnection<KeyCombination> GetConnection()
        {
            return Connection;
        }

        public override IConnection GetConnectionInterface()
        {
            return Connection;
        }

        public override void OnQualityChanged(int qualityLevel)
        {
            if (HasConnection())
            {
                Connection.OnQualityChanged(qualityLevel);
            }
        }
    }
}
