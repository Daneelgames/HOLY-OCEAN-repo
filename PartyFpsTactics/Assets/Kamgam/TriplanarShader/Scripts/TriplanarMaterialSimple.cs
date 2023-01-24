using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.TriplanarShader
{
    public class TriplanarMaterialSimple : TriplanarMaterial
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
    [CustomEditor(typeof(TriplanarMaterialSimple))]
    public class TriplanarMaterialSimpleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var setter = target as TriplanarMaterialSimple;

            if (GUILayout.Button("Reset Properties"))
            {
                setter.ResetProperties();
                EditorUtils.RepaintViews();
            }
        }
    }
#endif
}
