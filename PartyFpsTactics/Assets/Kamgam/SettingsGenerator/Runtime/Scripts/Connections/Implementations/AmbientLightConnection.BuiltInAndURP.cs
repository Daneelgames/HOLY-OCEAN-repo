// URP and HDRP use the same logic, thus they are within one file.
#if !KAMGAM_RENDER_PIPELINE_HDRP
using UnityEngine;

namespace Kamgam.SettingsGenerator
{
    public partial class AmbientLightConnection : Connection<float>
    {
        protected Color defaultAmbientColor;
        protected float defaultColorMaxIntensity;

        public AmbientLightConnection()
        {
            defaultAmbientColor = RenderSettings.ambientLight;
            defaultColorMaxIntensity = Mathf.Max(defaultAmbientColor.r, defaultAmbientColor.g, defaultAmbientColor.b);
        }

        /// <summary>
        /// It's close to what most people expect if they set a "Brightness or Gamma" value in a game.<br />
        /// Actually it is the ambient light intensity (0% to 100%). Think of it as a percentage with 50 as the default.
        /// </summary>
        /// <returns>The "intensity" value between 0% and 100%.</returns>
        public override float Get()
        {
            // BuiltIn or URP
            if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Skybox && RenderSettings.skybox != null)
            {
                // AmbientIntensity ranges from 0 to 8 (default 1).
                var mappedValue = MathUtils.MapWithAnchor(RenderSettings.ambientIntensity, 0f, 1f, 8f, 0f, 50f, 100f);
                return mappedValue;
            }
            else
            {
                var lightColor = RenderSettings.ambientLight;
                float colorIntensity = Mathf.Max(lightColor.r, lightColor.g, lightColor.b);

                // Color ranges from 0.01f to 2 (default from defaultColorIntensity). We avoid 0 to retain ratios between R,G and B.
                float mappedValue = MathUtils.MapWithAnchor(colorIntensity, 0.01f, defaultColorMaxIntensity, 2f, 0f, 50f, 100f);
                return mappedValue;
            }
        }

        /// <summary>
        /// Sets the "intensity" value between 0f and 100f (think of it as a percentage with 50 as the default).
        /// </summary>
        /// <param name="intensity"></param>
        public override void Set(float intensity)
        {
            // BuiltIn and URP do the same here
            if (RenderSettings.ambientMode == UnityEngine.Rendering.AmbientMode.Skybox && RenderSettings.skybox != null)
            {
                // AmbientIntensity ranges from 0 to 8 (default 1).
                var mappedValue = MathUtils.MapWithAnchor(intensity, 0f, 50f, 100f, 0f, 1f, 8f);
                RenderSettings.ambientIntensity = mappedValue;
            }
            else
            {
                // Color ranges from 0 to 2 (default from defaultColorIntensity). We avoid 0 to retain ratios between R,G and B.
                float desiredIntensity = MathUtils.MapWithAnchor(intensity, 0f, 50f, 100f, 0.01f, defaultColorMaxIntensity, 2f);

                var lightColor = RenderSettings.ambientLight;
                float maxIntensity = Mathf.Max(lightColor.r, lightColor.g, lightColor.b);

                float multiplier = desiredIntensity / maxIntensity;
                var newLightColor = new Color(
                    Mathf.Min(lightColor.r * multiplier, 2f),
                    Mathf.Min(lightColor.g * multiplier, 2f),
                    Mathf.Min(lightColor.b * multiplier, 2f)
                    );

                RenderSettings.ambientLight = newLightColor;
            }

            NotifyListenersIfChanged(intensity);
        }
    }
}
#endif