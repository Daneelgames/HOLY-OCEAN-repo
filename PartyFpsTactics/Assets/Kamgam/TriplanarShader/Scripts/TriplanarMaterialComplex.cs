using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.TriplanarShader
{
    public class TriplanarMaterialComplex : TriplanarMaterial
    {
        public Texture GetMainTexture()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.MainTexture, TriplanarMaterialProperties.MainTextureDefault);
        }

        public void SetMainTexture(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.MainTexture, texture);
        }

        public float GetMetallic()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.Metallic, TriplanarMaterialProperties.MetallicDefault);
        }

        public void SetMetallic(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.Metallic, value);
        }

        public float GetSmoothness()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.Smoothness, TriplanarMaterialProperties.SmoothnessDefault);
        }

        public void SetSmoothness(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.Smoothness, value);
        }

        public float GetOcclusion()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.Occlusion, TriplanarMaterialProperties.OcclusionDefault);
        }

        public void SetOcclusion(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.Occlusion, value);
        }

        public Texture GetMetallicMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.MainMetallicMap, TriplanarMaterialProperties.MainMetallicMapDefault);
        }

        public void SetMetallicMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.MainMetallicMap, texture);
        }

        public Texture GetNormalMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.MainNormalMap, TriplanarMaterialProperties.MainNormalMapDefault);
        }

        public void SetNormalMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.MainNormalMap, texture);
        }

        public Texture GetOcclusionMap()
        {
            return getProperty(PropertySetter.GetTexture, PropertyBlockSetter.GetTexture, TriplanarMaterialProperties.MainOcclusionMap, TriplanarMaterialProperties.MainOcclusionMapDefault);
        }

        public void SetOcclusionMap(Texture texture)
        {
            setProperty(PropertySetter.SetTexture, PropertyBlockSetter.ScheduleTexture, TriplanarMaterialProperties.MainOcclusionMap, texture);
        }

        public Vector2 GetTiling()
        {
            return GetTiling(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.MainTilingAndOffsetBuiltIn, TriplanarMaterialProperties.MainTiling, TriplanarMaterialProperties.MainTilingDefault);
        }

        public void SetTiling(Vector2 tiling)
        {
            SetTiling(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.MainTilingAndOffsetBuiltIn, TriplanarMaterialProperties.MainTiling, tiling);
        }

        public Vector2 GetOffset()
        {
            return GetOffset(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.MainTilingAndOffsetBuiltIn, TriplanarMaterialProperties.MainOffset, TriplanarMaterialProperties.MainOffsetDefault);
        }

        public void SetOffset(Vector2 offset)
        {
            SetOffset(PropertySetter, PropertyBlockSetter, Renderer, Mode, TriplanarMaterialProperties.MainTilingAndOffsetBuiltIn, TriplanarMaterialProperties.MainOffset, offset);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TriplanarMaterialComplex))]
    public class TriplanarMaterialComplexEditor : Editor
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
