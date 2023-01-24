using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.TriplanarShader
{
    public class TriplanarMaterialComplexXYZ : TriplanarMaterial
    {
        // X

        public Color GetXColor()
        {
            return getProperty(PropertySetter.GetColor, PropertyBlockSetter.GetColor, TriplanarMaterialProperties.XColor, TriplanarMaterialProperties.XColorDefault);
        }

        public void SetXColor(Color color)
        {
            setProperty(PropertySetter.SetColor, PropertyBlockSetter.ScheduleColor, TriplanarMaterialProperties.XColor, color);
        }

        public Texture GetXTexture()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.XTexture, TriplanarMaterialProperties.XTextureDefault);
        }

        public void SetXTexture(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.XTexture, texture);
        }

        public Texture GetXMetallicMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.XMetallicMap, TriplanarMaterialProperties.XMetallicMapDefault);
        }

        public void SetXMetallicMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.XMetallicMap, texture);
        }

        public Texture GetXNormalMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.XNormalMap, TriplanarMaterialProperties.XNormalMapDefault);
        }

        public void SetXNormalMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.XNormalMap, texture);
        }

        public Texture GetXOcclusionMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.XOcclusionMap, TriplanarMaterialProperties.XOcclusionMapDefault);
        }

        public void SetXOcclusionMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.XOcclusionMap, texture);
        }

        public float GetXMetallic()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.XMetallic, TriplanarMaterialProperties.XMetallicDefault);
        }

        public void SetXMetallic(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.XMetallic, value);
        }

        public float GetXSmoothness()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.XSmoothness, TriplanarMaterialProperties.XSmoothnessDefault);
        }

        public void SetXSmoothness(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.XSmoothness, value);
        }

        public float GetXOcclusion()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.XOcclusion, TriplanarMaterialProperties.XOcclusionDefault);
        }

        public void SetXOcclusion(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.XOcclusion, value);
        }

        public Vector2 GetXTiling()
        {
            return GetTiling(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.XTilingAndOffsetBuiltIn, TriplanarMaterialProperties.XTiling, TriplanarMaterialProperties.XTilingDefault);
        }

        public void SetXTiling(Vector2 tiling)
        {
            SetTiling(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.XTilingAndOffsetBuiltIn, TriplanarMaterialProperties.XTiling, tiling);
        }

        public Vector2 GetXOffset()
        {
            return GetOffset(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.XTilingAndOffsetBuiltIn, TriplanarMaterialProperties.XOffset, TriplanarMaterialProperties.XOffsetDefault);
        }

        public void SetXOffset(Vector2 offset)
        {
            SetOffset(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.XTilingAndOffsetBuiltIn, TriplanarMaterialProperties.XOffset, offset);
        }


        // Y

        public Color GetYColor()
        {
            return getProperty(PropertySetter.GetColor, PropertyBlockSetter.GetColor, TriplanarMaterialProperties.YColor, TriplanarMaterialProperties.YColorDefault);
        }

        public void SetYColor(Color color)
        {
            setProperty(PropertySetter.SetColor, PropertyBlockSetter.ScheduleColor, TriplanarMaterialProperties.YColor, color);
        }

        public Texture GetYTexture()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.YTexture, TriplanarMaterialProperties.YTextureDefault);
        }

        public void SetYTexture(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.YTexture, texture);
        }

        public Texture GetYMetallicMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.YMetallicMap, TriplanarMaterialProperties.YMetallicMapDefault);
        }

        public void SetYMetallicMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.YMetallicMap, texture);
        }

        public Texture GetYNormalMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.YNormalMap, TriplanarMaterialProperties.YNormalMapDefault);
        }

        public void SetYNormalMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.YNormalMap, texture);
        }

        public Texture GetYOcclusionMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.YOcclusionMap, TriplanarMaterialProperties.YOcclusionMapDefault);
        }

        public void SetYOcclusionMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.YOcclusionMap, texture);
        }

        public float GetYMetallic()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.YMetallic, TriplanarMaterialProperties.YMetallicDefault);
        }

        public void SetYMetallic(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.YMetallic, value);
        }

        public float GetYSmoothness()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.YSmoothness, TriplanarMaterialProperties.YSmoothnessDefault);
        }

        public void SetYSmoothness(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.YSmoothness, value);
        }

        public float GetYOcclusion()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.YOcclusion, TriplanarMaterialProperties.YOcclusionDefault);
        }

        public void SetYOcclusion(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.YOcclusion, value);
        }

        /// <summary>
        /// Returns the normalized top skew value (range 0 to 1).<br />
        /// Use GetYSkewRaw() if you need the raw value.
        /// </summary>
        /// <returns></returns>
        public float GetTopSkew()
        {
            float raw = GetTopSkewRaw();
            return TriplanarMaterialProperties.NormalizeTopSkew(raw);
        }

        /// <summary>
        /// Sets the top skew value based on a normalized input (range 0 to 1).<br />
        /// Use SetYSkewRaw() if you want to set the raw value.
        /// </summary>
        /// <param name="value">YSkew in normalized range (0.0f to 1.0f).</param>
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

        public Vector2 GetYTiling()
        {
            return GetTiling(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.YTilingAndOffsetBuiltIn, TriplanarMaterialProperties.YTiling, TriplanarMaterialProperties.YTilingDefault);
        }

        public void SetYTiling(Vector2 tiling)
        {
            SetTiling(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.YTilingAndOffsetBuiltIn, TriplanarMaterialProperties.YTiling, tiling);
        }

        public Vector2 GetYOffset()
        {
            return GetOffset(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.YTilingAndOffsetBuiltIn, TriplanarMaterialProperties.YOffset, TriplanarMaterialProperties.YOffsetDefault);
        }

        public void SetYOffset(Vector2 offset)
        {
            SetOffset(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.YTilingAndOffsetBuiltIn, TriplanarMaterialProperties.YOffset, offset);
        }



        // Z

        public Color GetZColor()
        {
            return getProperty(PropertySetter.GetColor, PropertyBlockSetter.GetColor, TriplanarMaterialProperties.ZColor, TriplanarMaterialProperties.ZColorDefault);
        }

        public void SetZColor(Color color)
        {
            setProperty(PropertySetter.SetColor, PropertyBlockSetter.ScheduleColor, TriplanarMaterialProperties.ZColor, color);
        }

        public Texture GetZTexture()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.ZTexture, TriplanarMaterialProperties.ZTextureDefault);
        }

        public void SetZTexture(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.ZTexture, texture);
        }

        public Texture GetZMetallicMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.ZMetallicMap, TriplanarMaterialProperties.ZMetallicMapDefault);
        }

        public void SetZMetallicMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.ZMetallicMap, texture);
        }

        public Texture GetZNormalMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.ZNormalMap, TriplanarMaterialProperties.ZNormalMapDefault);
        }

        public void SetZNormalMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.ZNormalMap, texture);
        }

        public Texture GetZOcclusionMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.ZOcclusionMap, TriplanarMaterialProperties.ZOcclusionMapDefault);
        }

        public void SetZOcclusionMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.ZOcclusionMap, texture);
        }

        public float GetZMetallic()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.ZMetallic, TriplanarMaterialProperties.ZMetallicDefault);
        }

        public void SetZMetallic(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.ZMetallic, value);
        }

        public float GetZSmoothness()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.ZSmoothness, TriplanarMaterialProperties.ZSmoothnessDefault);
        }

        public void SetZSmoothness(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.ZSmoothness, value);
        }

        public float GetZOcclusion()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.ZOcclusion, TriplanarMaterialProperties.ZOcclusionDefault);
        }

        public void SetZOcclusion(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.ZOcclusion, value);
        }

        public Vector2 GetZTiling()
        {
            return GetTiling(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.ZTilingAndOffsetBuiltIn, TriplanarMaterialProperties.ZTiling, TriplanarMaterialProperties.ZTilingDefault);
        }

        public void SetZTiling(Vector2 tiling)
        {
            SetTiling(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.ZTilingAndOffsetBuiltIn, TriplanarMaterialProperties.ZTiling, tiling);
        }

        public Vector2 GetZOffset()
        {
            return GetOffset(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.ZTilingAndOffsetBuiltIn, TriplanarMaterialProperties.ZOffset, TriplanarMaterialProperties.ZOffsetDefault);
        }

        public void SetZOffset(Vector2 offset)
        {
            SetOffset(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.ZTilingAndOffsetBuiltIn, TriplanarMaterialProperties.ZOffset, offset);
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TriplanarMaterialComplexXYZ))]
    public class TriplanarMaterialComplexXYZEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var setter = target as TriplanarMaterialComplex;

            if (GUILayout.Button("Reset Properties"))
            {
                setter.ResetProperties();
                EditorUtils.RepaintViews();
            }
        }
    }
#endif
}
