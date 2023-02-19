using UnityEngine;
using Kamgam.UGUIComponentsForSettings;

namespace Kamgam.SettingsGenerator
{
    [RequireComponent(typeof(ToggleUGUI))]
    public class ToggleUGUIResolver : SettingResolver, ISettingResolver
    {
        protected ToggleUGUI toggleUGUI;
        public ToggleUGUI ToggleUGUI
        {
            get
            {
                if (toggleUGUI == null)
                {
                    toggleUGUI = this.GetComponent<ToggleUGUI>();
                }
                return toggleUGUI;
            }
        }

        protected SettingData.DataType[] supportedDataTypes = new SettingData.DataType[] { SettingData.DataType.Bool };

        public override SettingData.DataType[] GetSupportedDataTypes()
        {
            return supportedDataTypes;
        }

        protected bool stopPropagation = false;

        public override void Start()
        {
            base.Start();

            ToggleUGUI.OnValueChanged += onValueChanged;

            if (HasValidSettingForID(ID, GetSupportedDataTypes()))
            {
                var setting = SettingsProvider.Settings.GetSetting(ID);
                setting.AddPulledFromConnectionListener(Refresh);

                Refresh();
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (ToggleUGUI != null)
                ToggleUGUI.OnValueChanged -= onValueChanged;
        }

        private void onValueChanged(bool value)
        {
            if (stopPropagation)
                return;

            if (!HasValidSettingForID(ID, GetSupportedDataTypes()))
                return;

            var setting = SettingsProvider.Settings.GetBool(ID);
            if (setting != null)
            {
                setting.SetValue(value);
            }
        }

        public override void Refresh()
        {
            if (!HasValidSettingForID(ID, GetSupportedDataTypes()))
                return;

            try
            {
                stopPropagation = true;

                var setting = SettingsProvider.Settings.GetBool(ID);
                if (setting != null)
                {
                    ToggleUGUI.Value = setting.GetValue();
                }
            }
            finally
            {
                stopPropagation = false;
            }
        }
    }
}
