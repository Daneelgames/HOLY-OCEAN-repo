using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.SettingsGenerator.Examples
{
    public class InputSystemBindingDemo : MonoBehaviour
    {
        public SettingsProvider Provider;

        public void Awake()
        {
            // We have to call the settings system at least once to initialize the load.
            var _ = Provider.Settings;
        }
    }
}
