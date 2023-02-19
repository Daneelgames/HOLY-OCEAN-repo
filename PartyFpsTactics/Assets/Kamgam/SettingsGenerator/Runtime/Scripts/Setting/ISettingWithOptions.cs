using System.Collections.Generic;

namespace Kamgam.SettingsGenerator
{
    public interface ISettingWithOptions<TOption> : ISettingWithConnection<int>
    {
        public bool HasOptions();
        public List<TOption> GetOptionLabels();
        public void SetOptionLabels(List<TOption> options);

        public bool GetOverrideConnectionLabels();
        public void SetOverrideConnectionLabels(bool overrideLabels);
    }
}
