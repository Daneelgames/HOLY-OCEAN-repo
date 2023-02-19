using System.Collections.Generic;

namespace Kamgam.SettingsGenerator
{
    public interface IConnectionWithOptions<TOption> : IConnection<int>
    {
        public bool HasOptions();
        public List<TOption> GetOptionLabels();
        public void SetOptionLabels(List<TOption> optionLabels);

        /// <summary>
        /// Fetches the options labels from the connection.
        /// </summary>
        public void RefreshOptionLabels();
    }
}
