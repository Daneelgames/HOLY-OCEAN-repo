using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.TriplanarShader
{
    /// <summary>
    /// Applies changes to a Material.<br />
    /// <br />
    /// NOTICE: Changes to .sharedMaterial in the EDITOR will
    /// change the Material Asset! Instead you should get a copy
    /// of the material with ".material" and then use and modify that
    /// instance (set it as .sharedMaterial).
    /// </summary>
    public class MaterialPropertySetter : MonoBehaviour
    {
        [Tooltip("Which renderer to use (if there are multiple).")]
        public Renderer Renderer;

        [Tooltip("Which material to use (if there are multiple).")]
        public int MaterialIndex = 0;

        /// <summary>
        /// Whether or not the .sharedMaterial should be used instead of .material.<br />
        /// <br />
        /// NOTICE: Changes to .sharedMaterial in the EDITOR will change the Material asset!<br />
        /// <br />
        /// If you want to modify only a single instance then you should either use
        /// MaterialPropertyBlocks or get a copy of the material with ".material" and
        /// then cache that instance and re-use it to modify the material. This is what
        /// this object will do if 'UseSharedMaterial' is disabled.
        /// </summary>
        [Tooltip("Whether or not the .sharedMaterial should be used instead of .material.\n\n" +
            "NOTICE: Changes to .sharedMaterial in the EDITOR will change the Material asset!\n\n" +
            "If you want to modify only a single instance then you should either use \n" +
            "MaterialPropertyBlocks or get a copy of the material with '.material# and \n" +
            "then cache that instance and re-use it to modify the material. This is what \n" +
            "this object will do if 'UseSharedMaterial' is disabled.")]
        protected bool _useSharedMaterial = false;
        public bool UseSharedMaterial
        {
            get => _useSharedMaterial;
            set
            {
                if (value == _useSharedMaterial)
                    return;

                _useSharedMaterial = value;

                // Refresh used material instance.
                CurrentlyUsedMaterialInstance = null;
                GetMaterial();
            }
        }

        /// <summary>
        /// Set this if you want to change the currently used material instance.<br />
        /// All changes are applied  to this Material instance.<br />
        /// Reset to NULL if you want to go back to the default behaviour.
        /// </summary>
        [System.NonSerialized]
        public Material CurrentlyUsedMaterialInstance;


        #region Default Texture Cache
        /// <summary>
        /// Caches results from the attempt to read the default texture color from
        /// the shader and create a texture.
        /// </summary>
        [System.NonSerialized]
        protected static Dictionary<Vector4, Texture2D> _defaultTextureCache;

        protected static Texture2D getDefaultTexture(Vector4 defaultTextureColor)
        {
            if (_defaultTextureCache == null)
            {
                _defaultTextureCache = new Dictionary<Vector4, Texture2D>();
            }

            if (_defaultTextureCache.ContainsKey(defaultTextureColor))
            {
                return _defaultTextureCache[defaultTextureColor];
            }
            else
            {
                var texture = new Texture2D(4, 4);
                for (int x = 0; x < texture.width; x++)
                {
                    for (int y = 0; y < texture.height; y++)
                    {
                        texture.SetPixel(x, y, defaultTextureColor);
                    }
                }
                texture.Apply(updateMipmaps: true, makeNoLongerReadable: true);
                _defaultTextureCache.Add(defaultTextureColor, texture);
                return texture;
            }
        }
        #endregion

        #region Default Texture Colors
        [Header("Texture Colors")]
        /// <summary>
        /// The color is used if a texture is set to NULL and no color was found in 'DefaultTextureColors'.
        /// </summary>
        [Tooltip("Use this to specify fallback color.\n\n" +
            "The color is used if a texture is set to NULL and no color was found in 'DefaultTextureColors'.")]
        public Color FallbackTextureColor = Color.black;

        [System.Serializable]
        public struct PropertyDefaultColor
        {
            public string ShaderPropertyName;
            public Color ShaderDefaultColor;
        }

        /// <summary>
        /// List of default texture colors. If a texture is set to NULL then these colors are used.<br />
        /// The Names need to match the shader texture properties.
        /// </summary>
        [Tooltip("List of default texture colors. If a texture is set to NULL then these colors are used.\n" +
                 "The names need to match the shader texture properties names.")]
        public PropertyDefaultColor[] DefaultTextureColors;

        public Color GetDefaultTextureColor(string propertyName)
        {
            if (DefaultTextureColors == null)
                return FallbackTextureColor;

            foreach (var defaultColor in DefaultTextureColors)
            {
                if (defaultColor.ShaderPropertyName == propertyName)
                    return defaultColor.ShaderDefaultColor;
            }

            return FallbackTextureColor;
        }
        #endregion

        [System.NonSerialized]
        protected Dictionary<string, float> _initialFloats;
        [System.NonSerialized]
        protected Dictionary<string, int> _initialInts;
        [System.NonSerialized]
        protected Dictionary<string, Vector4> _initialVector4s;
        [System.NonSerialized]
        protected Dictionary<string, Color> _initialColors;
        [System.NonSerialized]
        protected Dictionary<string, Texture> _initialTextures;
        [System.NonSerialized]
        protected Dictionary<string, Vector2> _initialTextureScales;
        [System.NonSerialized]
        protected Dictionary<string, Vector2> _initialTextureOffsets;

        public void Init()
        {
            if(Renderer == null)
                Renderer = this.GetComponentInChildren<Renderer>();
        }

#if UNITY_EDITOR
        public void Reset()
        {
            Init();
        }
#endif

        public Material GetSharedMaterial()
        {
            return Renderer.sharedMaterials[MaterialIndex];
        }

        public Material GetMaterial()
        {
            if (CurrentlyUsedMaterialInstance == null)
            {
                if (_useSharedMaterial)
                {
                    CurrentlyUsedMaterialInstance = GetSharedMaterial();
                }
                else
                {
#if UNITY_EDITOR
                    if (!EditorApplication.isPlaying)
                    {
                        Logger.LogWarning("Using shared material while in EDIT mode to avoid leaking materials.");
                        CurrentlyUsedMaterialInstance = GetSharedMaterial();
                        return CurrentlyUsedMaterialInstance;
                    }
#endif
                    // Create a copy of the material instance and store if for re-use.
                    CurrentlyUsedMaterialInstance = getNewMaterialInstance();
                }
            }

            return CurrentlyUsedMaterialInstance;
        }

        protected Material getNewMaterialInstance()
        {
            return Renderer.materials[MaterialIndex];
        }

        public float GetFloat(string propertyName, float defaultValue = default)
        {
            if (GetMaterial() == null)
                return defaultValue;

            return GetMaterial().GetFloat(propertyName);
        }

        public int GetInt(string propertyName, int defaultValue = default)
        {
            if (GetMaterial() == null)
                return defaultValue;

            return GetMaterial().GetInt(propertyName);
        }

        public Vector4 GetVector4(string propertyName, Vector4 defaultValue = default)
        {
            if (GetMaterial() == null)
                return defaultValue;

            return GetMaterial().GetVector(propertyName);
        }

        public Color GetColor(string propertyName, Color defaultValue = default)
        {
            if (GetMaterial() == null)
                return defaultValue;

            return GetMaterial().GetColor(propertyName);
        }

        public Texture GetTexture(string propertyName, Texture defaultValue = default)
        {
            if (GetMaterial() == null)
                return defaultValue;

            return GetMaterial().GetTexture(propertyName);
        }

        public Vector2 GetTextureScale(string propertyName, Vector2 defaultValue = default)
        {
            if (GetMaterial() == null)
                return defaultValue;

            return GetMaterial().GetTextureScale(propertyName);
        }

        public Vector2 GetTextureOffset(string propertyName, Vector2 defaultValue = default)
        {
            if (GetMaterial() == null)
                return defaultValue;

            return GetMaterial().GetTextureOffset(propertyName);
        }

        protected void rememberInitialValue<T>(ref Dictionary<string, T> dict, string propertyName, T value)
        {
            if (dict == null)
                dict = new Dictionary<string, T>();

            if (!dict.ContainsKey(propertyName))
            {
                dict.Add(propertyName, value);
            }
        }

        public void SetFloat(string propertyName, float value)
        {
            if (GetMaterial() == null)
                return;

            rememberInitialValue(ref _initialFloats, propertyName, GetFloat(propertyName, default));

            GetMaterial().SetFloat(propertyName, value);
        }

        public void SetInt(string propertyName, int value)
        {
            if (GetMaterial() == null)
                return;

            rememberInitialValue(ref _initialInts, propertyName, GetInt(propertyName, default));

            GetMaterial().SetInt(propertyName, value);
        }

        public void SetVector4(string propertyName, Vector4 value)
        {
            if (GetMaterial() == null)
                return;

            rememberInitialValue(ref _initialVector4s, propertyName, GetVector4(propertyName, default));

            GetMaterial().SetVector(propertyName, value);
        }

        public void SetColor(string propertyName, Color value)
        {
            if (GetMaterial() == null)
                return;

            rememberInitialValue(ref _initialColors, propertyName, GetColor(propertyName, default));

            GetMaterial().SetColor(propertyName, value);
        }

        public void SetTexture(string propertyName, Texture value)
        {
            if (GetMaterial() == null)
                return;

            rememberInitialValue(ref _initialTextures, propertyName, GetTexture(propertyName, default));

            if (value != null)
            {
                GetMaterial().SetTexture(propertyName, value);
            }
            else
            {
                // Problem 1: We can not use NULL as a value in propertyBlock.SetTexture().
                // Solution 1: Generate a texture based on the texture properties default color
                //             and use that instead.

                // Problem 2: We can not read the default color of a texture property (no API for that).
                // Workaround 2: Define a default color in the inspector.
                //               Caveat: This needs to be updated manually
                //                       if the used shader changes.
                var defaultColor = GetDefaultTextureColor(propertyName);
                var texture = getDefaultTexture(defaultColor);
                GetMaterial().SetTexture(propertyName, texture);
            }
        }

        public void SetTextureScale(string propertyName, Vector2 value)
        {
            if (GetMaterial() == null)
                return;

            rememberInitialValue(ref _initialTextureScales, propertyName, GetTextureScale(propertyName, default));

            GetMaterial().SetTextureScale(propertyName, value);
        }

        public void SetTextureOffset(string propertyName, Vector2 value)
        {
            if (GetMaterial() == null)
                return;

            rememberInitialValue(ref _initialTextureOffsets, propertyName, GetTextureOffset(propertyName, default));

            GetMaterial().SetTextureOffset(propertyName, value);
        }

        protected void applyList<T>(Dictionary<string, T> dict, System.Action<string, T> setter)
        {
            if (dict != null)
            {
                foreach (var kv in dict)
                {
                    setter.Invoke(kv.Key, kv.Value);
                }
            }
        }

        /// <summary>
        /// Clears the memory cache of  initial values. Use this if
        /// you change the used material.
        /// </summary>
        public void ClearInitialValues()
        {
            if (_initialFloats != null)     _initialFloats.Clear();
            if (_initialInts != null)       _initialInts.Clear();
            if (_initialVector4s != null)   _initialVector4s.Clear();
            if (_initialColors != null)     _initialColors.Clear();
            if (_initialTextures != null)   _initialTextures.Clear();
            if (_initialTextureScales != null) _initialTextureScales.Clear();
            if (_initialTextureOffsets != null) _initialTextureOffsets.Clear();
        }

        /// <summary>
        /// Resets the properties.<br />
        /// Does not affect the scheduled changes.
        /// It simply creates a new empty property block and applies that.
        /// All scheduled changes will remain scheduled.
        /// </summary>
        public void ResetProperties()
        {
            if (Renderer == null)
                throw new System.Exception("Renderer is null.");

            applyList(_initialFloats, SetFloat);
            applyList(_initialInts, SetInt);
            applyList(_initialVector4s, SetVector4);
            applyList(_initialColors, SetColor);
            applyList(_initialTextures, SetTexture);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MaterialPropertySetter))]
    public class MaterialPropertySetterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var setter = target as MaterialPropertySetter;

            if (GUILayout.Button("Reset Properties"))
            {
                setter.ResetProperties();
                EditorUtils.RepaintViews();
            }

            if (GUILayout.Button("Test: Apply random color to _Color"))
            {
                setter.SetColor("_Color", new Color(Random.value, Random.value, Random.value, Random.value));
                EditorUtils.RepaintViews();
            }
        }
    }
#endif
}
