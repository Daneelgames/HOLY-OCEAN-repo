using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.TriplanarShader
{
    public enum TriplanarMaterialMode
    {
        /// <summary>
        /// Use MaterialPropertyBlocks to change material properties.
        /// Use this if you need to make changes to many different instances.
        /// Using PropertyBlocks reduces draw calls.
        /// </summary>
        PropertyBlock,

        /// <summary>
        /// Use if you only need to change one instance.
        /// </summary>
        Instance,

        /// <summary>
        /// Use if you want to change the properties for all objects which
        /// use the same material.
        /// </summary>
        Shared
    }

    [RequireComponent(typeof(MaterialPropertySetter), typeof(MaterialPropertyBlockSetter))]
    public abstract class TriplanarMaterial : MonoBehaviour
    {
        /// <summary>
        /// How to change the material.<br />
        /// See: TriplanarPropertySetter.MaterialMode.
        /// </summary>
        [SerializeField]
        protected TriplanarMaterialMode _mode = TriplanarMaterialMode.PropertyBlock;
        public TriplanarMaterialMode Mode
        {
            get => _mode;
            set
            {
                if (value == _mode)
                    return;

                // If we are changing away from PropertyBlock to another method then we have
                // to reset the PropertyBlocks. Otherwise they would keep overriding the
                // changes made to the material.
                if (_mode == TriplanarMaterialMode.PropertyBlock)
                {
                    PropertyBlockSetter.ResetProperties();
                }

                // If we are changing away from Instance mode then we have to reset the used
                // Material Instance. Otherwise the newly created instance would be kept as the
                // currently used material (overriding any changes).
                if (_mode == TriplanarMaterialMode.Instance)
                {
                    PropertySetter.CurrentlyUsedMaterialInstance = null;
                }

                _mode = value;
            }
        }

        /// <summary>
        /// The renderer which to apply the changes to.<br />
        /// If no renderer is specified then the first available renderer in the hierarchy (GetComponentInChildren) will be used.
        /// </summary>
        [SerializeField]
        protected Renderer _renderer;
        public Renderer Renderer
        {
            get => _renderer;
            set
            {
                _renderer = value;
                PropertySetter.Renderer = value;
                PropertyBlockSetter.Renderer = value;
            }
        }

        /// <summary>
        /// The index of the material which to apply the changes to.<br />
        /// The index is based on the "materials" list of the used renderer.
        /// </summary>
        [SerializeField]
        protected int _materialIndex = 0;
        public int MaterialIndex
        {
            get => _materialIndex;
            set
            {
                _materialIndex = value;
                PropertySetter.MaterialIndex = value;
                PropertyBlockSetter.MaterialIndex = value;
            }
        }

        protected MaterialPropertySetter _propertySetter;
        public MaterialPropertySetter PropertySetter
        {
            get
            {
                if (_propertySetter == null)
                {
                    _propertySetter = this.GetComponent<MaterialPropertySetter>();
                    _propertySetter.MaterialIndex = MaterialIndex;
                }
                return _propertySetter;
            }
        }

        protected MaterialPropertyBlockSetter _propertyBlockSetter;
        public MaterialPropertyBlockSetter PropertyBlockSetter
        {
            get
            {
                if (_propertyBlockSetter == null)
                {
                    _propertyBlockSetter = this.GetComponent<MaterialPropertyBlockSetter>();
                    _propertyBlockSetter.MaterialIndex = MaterialIndex;
                }
                return _propertyBlockSetter;
            }
        }

        public void FindRenderer()
        {
            if (Renderer == null)
                Renderer = this.GetComponentInChildren<Renderer>();
        }

#if UNITY_EDITOR
        public void Reset()
        {
            FindRenderer();
        }


        private TriplanarMaterialMode _lastKnownMode = TriplanarMaterialMode.PropertyBlock;
        private int _lastKnownMaterialIndex = 0;
        private Renderer _lastKnownRenderer = null;

        public void OnValidate()
        {
            // Ensure the setter is triggered if the mode is changed via inspector.
            if (_lastKnownMode != Mode)
            {
                var tmpMode = Mode;
                _mode = _lastKnownMode;
                Mode = tmpMode; // trigger the property setter
                _lastKnownMode = Mode;
            }

            // Ensure the setter is triggered if the material index is changed via inspector.
            if (_lastKnownMaterialIndex != MaterialIndex)
            {
                var tmpMaterialIndex = MaterialIndex;
                _materialIndex = _lastKnownMaterialIndex;
                MaterialIndex = tmpMaterialIndex; // trigger the property setter
                _lastKnownMaterialIndex = MaterialIndex;
            }

            // Ensure the setter is triggered if the material index is changed via inspector.
            if (_lastKnownRenderer != Renderer)
            {
                var tmpRenderer = Renderer;
                _renderer = _lastKnownRenderer;
                Renderer = tmpRenderer; // trigger the property setter
                _lastKnownRenderer = Renderer;
            }

            if (!EditorApplication.isPlaying && Mode == TriplanarMaterialMode.Instance)
            {
                Logger.LogMessage("Instance mode can only be used in PLAY mode. Resetting to PropertyBlock Mode.");
                Mode = TriplanarMaterialMode.PropertyBlock;
            }
        }
#endif

        public void OnEnable()
        {
            FindRenderer();
        }

        public static T GetProperty<T>(
            MaterialPropertySetter propertySetter,
            Renderer renderer,
            TriplanarMaterialMode mode,
            System.Func<string, T, T> getterProperty, System.Func<string, T, T> getterPropertyBlock, string propertyName, T defaultValue)
        {
            if (renderer == null)
                return defaultValue;

            switch (mode)
            {
                case TriplanarMaterialMode.PropertyBlock:
                    return getterPropertyBlock(propertyName, defaultValue);

                case TriplanarMaterialMode.Instance:
                    propertySetter.UseSharedMaterial = false;
                    return getterProperty(propertyName, defaultValue);

                case TriplanarMaterialMode.Shared:
                default:
                    propertySetter.UseSharedMaterial = true;
                    return getterProperty(propertyName, defaultValue);
            }
        }

        public static void SetProperty<T>(
            MaterialPropertySetter propertySetter,
            MaterialPropertyBlockSetter propertyBlockSetter,
            Renderer Renderer,
            TriplanarMaterialMode mode,
            System.Action<string, T> setterProperty, System.Action<string, T> setterPropertyBlock, string propertyName, T value)
        {
            if (Renderer == null)
                return;

            switch (mode)
            {
                case TriplanarMaterialMode.PropertyBlock:
                    setterPropertyBlock(propertyName, value);
                    propertyBlockSetter.Apply();
                    break;

                case TriplanarMaterialMode.Instance:
                    propertySetter.UseSharedMaterial = false;
                    setterProperty(propertyName, value);
                    break;

                case TriplanarMaterialMode.Shared:
                default:
                    propertySetter.UseSharedMaterial = true;
                    setterProperty(propertyName, value);
                    break;
            }
        }

        protected T getProperty<T>(System.Func<string, T, T> getterProperty, System.Func<string, T, T> getterPropertyBlock, string propertyName, T defaultValue)
        {
            return GetProperty(PropertySetter, Renderer, Mode, getterProperty, getterPropertyBlock, propertyName, defaultValue);
        }

        protected void setProperty<T>(System.Action<string, T> setterProperty, System.Action<string, T> setterPropertyBlock, string propertyName, T value)
        {
            SetProperty(PropertySetter, PropertyBlockSetter, Renderer, Mode, setterProperty, setterPropertyBlock, propertyName, value);
        }

        public Color GetColor()
        {
            return getProperty(PropertySetter.GetColor, PropertyBlockSetter.GetColor, TriplanarMaterialProperties.Color, TriplanarMaterialProperties.ColorDefault);
        }

        public void SetColor(Color color)
        {
            setProperty(PropertySetter.SetColor, PropertyBlockSetter.ScheduleColor, TriplanarMaterialProperties.Color, color);
        }

        /// <summary>
        /// Returns the normalized blend value (range 0 to 1).<br />
        /// Use GetBlendRaw() if you need the raw value.
        /// </summary>
        /// <returns></returns>
        public float GetBlend()
        {
            float raw = GetBlendRaw();
            return TriplanarMaterialProperties.NormalizeBlend(raw);
        }

        /// <summary>
        /// Sets the blend value based on a normalized input (range 0 to 1).<br />
        /// Use SetBlendRaw() if you want to set the raw value.
        /// </summary>
        /// <param name="value">Blend in normalized range (0.0f to 1.0f).</param>
        public void SetBlend(float value)
        {
            float raw = TriplanarMaterialProperties.DenormalizeBlend(value);
            SetBlendRaw(raw);
        }

        public float GetBlendRaw()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.Blend, TriplanarMaterialProperties.BlendDefault);
        }

        public void SetBlendRaw(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.Blend, value);
        }

        public Color GetEmission()
        {
            return getProperty(PropertySetter.GetColor, PropertyBlockSetter.GetColor, TriplanarMaterialProperties.Emission, TriplanarMaterialProperties.EmissionDefault);
        }

        public void SetEmission(Color color)
        {
            setProperty(PropertySetter.SetColor, PropertyBlockSetter.ScheduleColor, TriplanarMaterialProperties.Emission, color);
        }

        public float GetEmissionIntensity()
        {
            return getProperty(PropertySetter.GetFloat, PropertyBlockSetter.GetFloat, TriplanarMaterialProperties.EmissionIntensity, TriplanarMaterialProperties.EmissionIntensityDefault);
        }

        public void SetEmissionIntensity(float value)
        {
            setProperty(PropertySetter.SetFloat, PropertyBlockSetter.ScheduleFloat, TriplanarMaterialProperties.EmissionIntensity, value);
        }

        public void ResetProperties()
        {
            if (Renderer == null)
                return;

            PropertyBlockSetter.ResetProperties();
            PropertyBlockSetter.ClearScheduled();
            PropertySetter.ResetProperties();
        }

        public static void SetOffset(MaterialPropertySetter propertySetter, MaterialPropertyBlockSetter propertyBlockSetter, Renderer renderer, TriplanarMaterialMode mode, string propertyNameBuiltIn, string propertyNameRP, Vector2 offset)
        {
            // Offset needs a different handling based on the used render pipleline

            bool isBuiltIn = GraphicsSettings.currentRenderPipeline == null;
            if (isBuiltIn)
            {
                // In Built-In tiling and offset can only be set together in surface shaders.
                var tiling = GetTiling(propertySetter, propertyBlockSetter, renderer, mode, propertyNameBuiltIn, propertyNameRP, Vector4.one);
                var value = new Vector4(tiling.x, tiling.y, offset.x, offset.y);
                SetProperty(propertySetter, propertyBlockSetter, renderer, mode, propertySetter.SetVector4, propertyBlockSetter.ScheduleVector4, propertyNameBuiltIn, value);
            }
            else
            {
                // Shadergraph does not have a Vector2 type. Only the first two values are actually used.
                var value = new Vector4(offset.x, offset.y, 0, 0);
                SetProperty(propertySetter, propertyBlockSetter, renderer, mode, propertySetter.SetVector4, propertyBlockSetter.ScheduleVector4, propertyNameRP, value);
            }
        }

        public static Vector2 GetOffset(MaterialPropertySetter propertySetter, MaterialPropertyBlockSetter propertyBlockSetter, Renderer renderer, TriplanarMaterialMode mode, string propertyNameBuiltIn, string propertyNameRP, Vector2 defaultValue)
        {
            // Offset needs a different handling per render pipleline

            bool isBuiltIn = GraphicsSettings.currentRenderPipeline == null;
            if (isBuiltIn)
            {
                var st = GetProperty(propertySetter, renderer, mode, propertySetter.GetVector4, propertyBlockSetter.GetVector4, propertyNameBuiltIn, new Vector4(1f, 1f, defaultValue.x, defaultValue.y));
                return new Vector2(st.z, st.w);
            }
            else
            {
                // Shadergraph does not have a Vector2 type. Only the first two values are actually used.
                var t = GetProperty(propertySetter, renderer, mode, propertySetter.GetVector4, propertyBlockSetter.GetVector4, propertyNameRP, new Vector4(defaultValue.x, defaultValue.y, 1f, 1f));
                return new Vector2(t.x, t.y);
            }
        }

        public static void SetTiling(MaterialPropertySetter propertySetter, MaterialPropertyBlockSetter propertyBlockSetter, Renderer renderer, TriplanarMaterialMode mode, string propertyNameBuiltIn, string propertyNameRP, Vector2 tiling)
        {
            // Tiling and offset need different handling based on the used render pipleline

            bool isBuiltIn = GraphicsSettings.currentRenderPipeline == null;
            if (isBuiltIn)
            {
                // In Built-In tiling and offset can only be set together in surface shaders.
                var offset = GetOffset(propertySetter, propertyBlockSetter, renderer, mode, propertyNameBuiltIn, propertyNameRP, Vector4.zero);
                var value = new Vector4(tiling.x, tiling.y, offset.x, offset.y);
                SetProperty(propertySetter, propertyBlockSetter, renderer, mode, propertySetter.SetVector4, propertyBlockSetter.ScheduleVector4, propertyNameBuiltIn, value);
            }
            else
            {
                // Shadergraph does not have a Vector2 type. Only the first two values are actually used.
                var value = new Vector4(tiling.x, tiling.y, 1, 1);
                SetProperty(propertySetter, propertyBlockSetter, renderer, mode, propertySetter.SetVector4, propertyBlockSetter.ScheduleVector4, propertyNameRP, value);
            }
        }

        public static Vector2 GetTiling(MaterialPropertySetter propertySetter, MaterialPropertyBlockSetter propertyBlockSetter, Renderer renderer, TriplanarMaterialMode mode, string propertyNameBuiltIn, string propertyNameRP, Vector2 defaultValue)
        {
            // Tiling and offset need different handling per render pipleline

            bool isBuiltIn = GraphicsSettings.currentRenderPipeline == null;
            if (isBuiltIn)
            {
                var st = GetProperty(propertySetter, renderer, mode, propertySetter.GetVector4, propertyBlockSetter.GetVector4, propertyNameBuiltIn, new Vector4(defaultValue.x, defaultValue.y, 1f, 1f));
                return new Vector2(st.x, st.y);
            }
            else
            {
                var t = GetProperty(propertySetter, renderer, mode, propertySetter.GetVector4, propertyBlockSetter.GetVector4, propertyNameRP, new Vector4(defaultValue.x, defaultValue.y, 1f, 1f));
                return new Vector2(t.x, t.y);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TriplanarMaterial))]
    public class TriplanarMaterialEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var setter = target as TriplanarMaterial;

            if (GUILayout.Button("Reset Properties"))
            {
                setter.ResetProperties();
                EditorUtils.RepaintViews();
            }

            if (GUILayout.Button("Test: Apply random color to _Color"))
            {
                setter.SetColor(new Color(Random.value, Random.value, Random.value, Random.value));
                EditorUtils.RepaintViews();
            }
        }
    }
#endif
}
