using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.TriplanarShader
{
    public class TriplanarMaterialSimpleTMB : TriplanarMaterial
    {
        // Main
        public Color GetMainColor()
        {
            return getProperty(PropertySetter.GetColor, PropertyBlockSetter.GetColor, TriplanarMaterialProperties.MainColor, TriplanarMaterialProperties.MainColorDefault);
        }

        public void SetMainColor(Color color)
        {
            setProperty(PropertySetter.SetColor, PropertyBlockSetter.ScheduleColor, TriplanarMaterialProperties.MainColor, color);
        }

        public Texture GetMainTexture()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.MainTexture, TriplanarMaterialProperties.MainTextureDefault);
        }

        public void SetMainTexture(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.MainTexture, texture);
        }

        public Texture GetMainMetallicMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.MainMetallicMap, TriplanarMaterialProperties.MainMetallicMapDefault);
        }

        public void SetMainMetallicMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.MainMetallicMap, texture);
        }

        public Texture GetMainNormalMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.MainNormalMap, TriplanarMaterialProperties.MainNormalMapDefault);
        }

        public void SetMainNormalMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.MainNormalMap, texture);
        }

        public Texture GetMainOcclusionMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.MainOcclusionMap, TriplanarMaterialProperties.MainOcclusionMapDefault);
        }

        public void SetMainOcclusionMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.MainOcclusionMap, texture);
        }

        public float GetMainMetallic()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.MainMetallic, TriplanarMaterialProperties.MainMetallicDefault);
        }

        public void SetMainMetallic(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.MainMetallic, value);
        }

        public float GetMainSmoothness()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.MainSmoothness, TriplanarMaterialProperties.MainSmoothnessDefault);
        }

        public void SetMainSmoothness(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.MainSmoothness, value);
        }

        public float GetMainOcclusion()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.MainOcclusion, TriplanarMaterialProperties.MainOcclusionDefault);
        }

        public void SetMainOcclusion(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.MainOcclusion, value);
        }

        public Vector2 GetMainTiling()
        {
            return GetTiling(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.MainTilingAndOffsetBuiltIn, TriplanarMaterialProperties.MainTiling, TriplanarMaterialProperties.MainTilingDefault);
        }

        public void SetMainTiling(Vector2 tiling)
        {
            SetTiling(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.MainTilingAndOffsetBuiltIn, TriplanarMaterialProperties.MainTiling, tiling);
        }

        public Vector2 GetMainOffset()
        {
            return GetOffset(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.MainTilingAndOffsetBuiltIn, TriplanarMaterialProperties.MainOffset, TriplanarMaterialProperties.MainOffsetDefault);
        }

        public void SetMainOffset(Vector2 offset)
        {
            SetOffset(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.MainTilingAndOffsetBuiltIn, TriplanarMaterialProperties.MainOffset, offset);
        }


        // TOP

        public Color GetTopColor()
        {
            return getProperty(PropertySetter.GetColor, PropertyBlockSetter.GetColor, TriplanarMaterialProperties.TopColor, TriplanarMaterialProperties.TopColorDefault);
        }

        public void SetTopColor(Color color)
        {
            setProperty(PropertySetter.SetColor, PropertyBlockSetter.ScheduleColor, TriplanarMaterialProperties.TopColor, color);
        }

        public Texture GetTopTexture()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.TopTexture, TriplanarMaterialProperties.TopTextureDefault);
        }

        public void SetTopTexture(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.TopTexture, texture);
        }

        public Texture GetTopMetallicMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.TopMetallicMap, TriplanarMaterialProperties.TopMetallicMapDefault);
        }

        public void SetTopMetallicMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.TopMetallicMap, texture);
        }

        public Texture GetTopNormalMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.TopNormalMap, TriplanarMaterialProperties.TopNormalMapDefault);
        }

        public void SetTopNormalMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.TopNormalMap, texture);
        }

        public Texture GetTopOcclusionMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.TopOcclusionMap, TriplanarMaterialProperties.TopOcclusionMapDefault);
        }

        public void SetTopOcclusionMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.TopOcclusionMap, texture);
        }

        public float GetTopMetallic()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.TopMetallic, TriplanarMaterialProperties.TopMetallicDefault);
        }

        public void SetTopMetallic(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.TopMetallic, value);
        }

        public float GetTopSmoothness()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.TopSmoothness, TriplanarMaterialProperties.TopSmoothnessDefault);
        }

        public void SetTopSmoothness(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.TopSmoothness, value);
        }

        public float GetTopOcclusion()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.TopOcclusion, TriplanarMaterialProperties.TopOcclusionDefault);
        }

        public void SetTopOcclusion(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.TopOcclusion, value);
        }

        /// <summary>
        /// Returns the normalized top skew value (range 0 to 1).<br />
        /// Use GetTopSkewRaw() if you need the raw value.
        /// </summary>
        /// <returns></returns>
        public float GetTopSkew()
        {
            float raw = GetTopSkewRaw();
            return TriplanarMaterialProperties.NormalizeTopSkew(raw);
        }

        /// <summary>
        /// Sets the top skew value based on a normalized input (range 0 to 1).<br />
        /// Use SetTopSkewRaw() if you want to set the raw value.
        /// </summary>
        /// <param name="value">TopSkew in normalized range (0.0f to 1.0f).</param>
        public void SetTopSkew(float value)
        {
            float raw = TriplanarMaterialProperties.DenormalizeTopSkew(value);
            SetTopSkewRaw(raw);
        }

        public float GetTopSkewRaw()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.TopSkew, TriplanarMaterialProperties.TopSkewDefault);
        }

        public void SetTopSkewRaw(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.TopSkew, value);
        }

        public Vector2 GetTopTiling()
        {
            return GetTiling(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.TopTilingAndOffsetBuiltIn, TriplanarMaterialProperties.TopTiling, TriplanarMaterialProperties.TopTilingDefault);
        }

        public void SetTopTiling(Vector2 tiling)
        {
            SetTiling(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.TopTilingAndOffsetBuiltIn, TriplanarMaterialProperties.TopTiling, tiling);
        }

        public Vector2 GetTopOffset()
        {
            return GetOffset(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.TopTilingAndOffsetBuiltIn, TriplanarMaterialProperties.TopOffset, TriplanarMaterialProperties.TopOffsetDefault);
        }

        public void SetTopOffset(Vector2 offset)
        {
            SetOffset(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.TopTilingAndOffsetBuiltIn, TriplanarMaterialProperties.TopOffset, offset);
        }



        // Bottom

        public Color GetBottomColor()
        {
            return getProperty(PropertySetter.GetColor, PropertyBlockSetter.GetColor, TriplanarMaterialProperties.BottomColor, TriplanarMaterialProperties.BottomColorDefault);
        }

        public void SetBottomColor(Color color)
        {
            setProperty(PropertySetter.SetColor, PropertyBlockSetter.ScheduleColor, TriplanarMaterialProperties.BottomColor, color);
        }

        public Texture GetBottomTexture()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.BottomTexture, TriplanarMaterialProperties.BottomTextureDefault);
        }

        public void SetBottomTexture(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.BottomTexture, texture);
        }

        public Texture GetBottomMetallicMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.BottomMetallicMap, TriplanarMaterialProperties.BottomMetallicMapDefault);
        }

        public void SetBottomMetallicMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.BottomMetallicMap, texture);
        }

        public Texture GetBottomNormalMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.BottomNormalMap, TriplanarMaterialProperties.BottomNormalMapDefault);
        }

        public void SetBottomNormalMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.BottomNormalMap, texture);
        }

        public Texture GetBottomOcclusionMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.BottomOcclusionMap, TriplanarMaterialProperties.BottomOcclusionMapDefault);
        }

        public void SetBottomOcclusionMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.BottomOcclusionMap, texture);
        }

        public float GetBottomMetallic()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.BottomMetallic, TriplanarMaterialProperties.BottomMetallicDefault);
        }

        public void SetBottomMetallic(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.BottomMetallic, value);
        }

        public float GetBottomSmoothness()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.BottomSmoothness, TriplanarMaterialProperties.BottomSmoothnessDefault);
        }

        public void SetBottomSmoothness(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.BottomSmoothness, value);
        }

        public float GetBottomOcclusion()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.BottomOcclusion, TriplanarMaterialProperties.BottomOcclusionDefault);
        }

        public void SetBottomOcclusion(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.BottomOcclusion, value);
        }

        public Vector2 GetBottomTiling()
        {
            return GetTiling(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.BottomTilingAndOffsetBuiltIn, TriplanarMaterialProperties.BottomTiling, TriplanarMaterialProperties.BottomTilingDefault);
        }

        public void SetBottomTiling(Vector2 tiling)
        {
            SetTiling(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.BottomTilingAndOffsetBuiltIn, TriplanarMaterialProperties.BottomTiling, tiling);
        }

        public Vector2 GetBottomOffset()
        {
            return GetOffset(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.BottomTilingAndOffsetBuiltIn, TriplanarMaterialProperties.BottomOffset, TriplanarMaterialProperties.BottomOffsetDefault);
        }

        public void SetBottomOffset(Vector2 offset)
        {
            SetOffset(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.BottomTilingAndOffsetBuiltIn, TriplanarMaterialProperties.BottomOffset, offset);
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TriplanarMaterialSimpleTMB))]
    public class TriplanarMaterialSimpleTMBEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var setter = target as TriplanarMaterialSimpleTMB;

            if (GUILayout.Button("Reset Properties"))
            {
                setter.ResetProperties();
                EditorUtils.RepaintViews();
            }
        }
    }
#endif
}
