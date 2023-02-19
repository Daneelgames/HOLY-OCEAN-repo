using System.Collections.Generic;

namespace Kamgam.SettingsGenerator
{
    public interface ISettingWithValue<TValue> : ISetting
    {
        public TValue GetValue();
        public void SetValue(TValue value, bool propagateChange = true);
        public void SetDefaultFromConnection(IConnection<TValue> connection);

        public void AddChangeListener(System.Action<TValue> onChanged);
        public void RemoveChangeListener(System.Action<TValue> onChanged);

        public void AddPulledFromConnectionListener(System.Action<TValue> onPulled);
        public void RemovePulledFromConnectionListener(System.Action<TValue> onPulled);
    }
}
