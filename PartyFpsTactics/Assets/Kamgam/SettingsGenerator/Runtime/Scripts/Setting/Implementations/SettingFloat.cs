using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.SettingsGenerator
{
    [System.Serializable]
    public class SettingFloat : SettingWithValue<float>, ISettingWithConnection<float>
    {
        public const SettingData.DataType DataType = SettingData.DataType.Float;

        [System.NonSerialized]
        public IConnection<float> Connection;

        [SerializeField]
        FloatConnectionSO ConnectionObject;

        [System.NonSerialized]
        protected float _value;

        public override float GetValue()
        {
            return _value;
        }

        protected bool _valueInitialized = false;

        public override void SetValue(float value, bool propagateChange = true)
        {
            if (_value == value && _valueInitialized)
                return;
            _valueInitialized = true;

            _value = value;

            if (propagateChange)
            {
                OnChanged();

                if (ApplyImmediately)
                    PushToConnection();
            }
        }

        public SettingFloat(SettingData data, List<string> groups = null) : base(data, groups) { }

        public SettingFloat(string path, float value, List<string> groups = null) : base(path, groups)
        {
            SetValue(value);
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
            SetValue((float)value, propagateChange);
        }

        public override SettingData.DataType GetDataType()
        {
            return DataType;
        }

        public override SettingData SerializeValueToData()
        {
            var data = new SettingData(ID, DataType);

            // serialize into primitives
            data.FloatValues = new float[] { GetValue() };

            return data;
        }

        public override void DeserializeValueFromData(SettingData data)
        {
            if (!checkDataType(data.Type, DataType))
                return;

            // deserialize from primitives
            SetValue(data.FloatValues[0], propagateChange: false);
        }

        protected void extractConnectionFromObject()
        {
            if (Connection == null && ConnectionObject != null)
            {
                if (ConnectionObject is FloatConnectionSO typedObject)
                {
                    Connection = typedObject.GetConnection();
                }
            }
        }

        public override bool HasConnection()
        {
            extractConnectionFromObject();
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

        public void SetConnection(IConnection<float> connection)
        {
            Connection = connection;

            if (connection != null)
            {
                InitializeConnection();
            }
        }

        public IConnection<float> GetConnection()
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
