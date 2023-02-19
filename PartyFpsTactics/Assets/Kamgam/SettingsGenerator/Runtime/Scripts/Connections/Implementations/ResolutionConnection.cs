using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.SettingsGenerator
{
    public class ResolutionConnection : ConnectionWithOptions<string>
    {
        /// <summary>
        /// It is not advisable to change the resolution on mobile.
        /// It may have unexpected sideeffect and there usually is
        /// just one anyways.
        /// 
        /// </summary>
        public static bool AllowResolutionChangeOnMobile = false;

        protected List<Resolution> _values;
        protected List<string> _labels;

        protected List<Resolution> getUniqueResolutions()
        {
            if (_values.IsNullOrEmpty())
            {
                _values = new List<Resolution>();

                // Generate a list of resolutions which have the same refresh rate as the current one.
                var resolutions = Screen.resolutions;
                foreach (var res in resolutions)
                {
#if UNITY_2022_2_OR_NEWER
                    if (Screen.currentResolution.refreshRateRatio.Equals(res.refreshRateRatio))
#else
                    // Weirdly sometimes the current refreshrate is 59 but the rate in all resolutions is 60.
                    // To avoid empty resolution lists we allow +/-1.
                    if (Mathf.Abs(Screen.currentResolution.refreshRate - res.refreshRate) <= 1)
#endif
                    {
                        _values.Add(res);
                    }
                }

                // Hard fallback
                if (_values.Count == 0)
                {
                    var res = new Resolution();
                    res.width = 1024;
                    res.height = 768;
#if UNITY_2022_2_OR_NEWER
                    var r = new RefreshRate();
                    r.numerator = 60000;
                    r.denominator = 1001;
                    res.refreshRateRatio = r;
#else
                    res.refreshRate = 60;
#endif
                    _values.Add(res);
                }
            }

            return _values;
        }

        public override List<string> GetOptionLabels()
        {
            if (_labels.IsNullOrEmpty())
            {
                _labels = new List<string>();

                var resolutions = getUniqueResolutions();
                foreach (var res in resolutions)
                {
                    string name = res.width + "x" + res.height;
                    _labels.Add(name);
                }
            }

            return _labels;
        }

        public override void RefreshOptionLabels()
        {
            _labels = null;
            GetOptionLabels();
        }

        public override void SetOptionLabels(List<string> optionLabels)
        {
            var resolutions = getUniqueResolutions();
            if (optionLabels == null || optionLabels.Count != resolutions.Count)
            {
                Logger.LogError("Invalid new labels. Need to be " + resolutions.Count + ".");
            }

            _labels = new List<string>(optionLabels);
        }

        protected Resolution? lastKnownResolution = null;
        protected int lastSetFrame = 0;

        public override int Get()
        {
            // Reset after N frames. The assumption is that
            // after that the Screen.currentResolution has been updated.
            if (Time.frameCount - lastSetFrame > 3)
                lastKnownResolution = null;

            Resolution currentResolution = Screen.currentResolution;
            if (lastKnownResolution.HasValue)
                currentResolution = lastKnownResolution.Value;

            var resolutions = getUniqueResolutions();
            for (int i = 0; i < resolutions.Count; i++)
            {
                if (resolutions[i].width == currentResolution.width
                    && resolutions[i].height == currentResolution.height)
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>
        /// NOTICE: This has no effect in the Edtior.<br />
        /// NOTICE: A resolution switch does not happen immediately; it happens when the current frame is finished.<br />
        /// See: https://docs.unity3d.com/ScriptReference/Screen.SetResolution.html
        /// </summary>
        /// <param name="index"></param>
        public override void Set(int index)
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_SWITCH
            if (!AllowResolutionChangeOnMobile)
            {
                Debug.LogWarning("Allow resolution change on mobile is disabled. It is not advisable to change the resolution on mobile. It may have unexpected sideeffects and there usually is just one anyways. If you are on URP then use the renderScale instead.");
                return;
            }
#endif

            var resolutions = getUniqueResolutions();
            index = Mathf.Clamp(index, 0, Mathf.Max(0, resolutions.Count - 1));
            var resolution = resolutions[index];

            // Request change but delegate the actual execution to the orchestrator.
            ScreenOrchestrator.Instance.RequestResolution(resolution);

            // remember
            lastSetFrame = Time.frameCount;
            lastKnownResolution = resolution;

            NotifyListenersIfChanged(index);

#if UNITY_EDITOR
            if (SettingsGeneratorSettings.GetOrCreateSettings().ShowEditorInfoLogs)
            {
                Logger.LogMessage("Setting the resolution has no effect in the Editor. Please try in a build. - " + SettingsGeneratorSettings._showEditorInfoLogsHint);
            }
#endif
        }
    }
}
