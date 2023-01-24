using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kamgam.TriplanarShader
{
    /// <summary>
    /// Caches and then applies changes to a Material by using MaterialPropertyBlocks.
    /// <br />
    /// Use it in situations where you want to draw multiple objects with
    /// the same material, but slightly different properties.
    /// <br />
    /// See: https://docs.unity3d.com/ScriptReference/MaterialPropertyBlock.html
    /// <br />
    /// <br />
    /// I works by scheduling changes. You will have to call Apply() to
    /// apply these changes to the material. This is a side-effect of how the 
    /// MaterialProperties work (all changes are pushed at once).
    /// </summary>
    public class MaterialPropertyBlockSetter : MonoBehaviour
    {
        public Renderer Renderer;
        public int MaterialIndex = 0;

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
        protected Dictionary<string, float> _scheduledFloats;
        [System.NonSerialized]
        protected Dictionary<string, int> _scheduledInts;
        [System.NonSerialized]
        protected Dictionary<string, Vector4> _scheduledVector4s;
        [System.NonSerialized]
        protected Dictionary<string, Color> _scheduledColors;
        [System.NonSerialized]
        protected Dictionary<string, Texture> _scheduledTextures;

        [System.NonSerialized]
        protected MaterialPropertyBlock _propertyBlock;

        public void Init()
        {
            if (Renderer == null)
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

        /// <summary>
        /// Are there any scheduled changes?
        /// </summary>
        /// <returns></returns>
        public bool HasScheduledChanges()
        {
            return (_scheduledFloats != null && _scheduledFloats.Count > 0)
                || (_scheduledInts != null && _scheduledInts.Count > 0)
                || (_scheduledVector4s != null && _scheduledVector4s.Count > 0)
                || (_scheduledColors != null && _scheduledColors.Count > 0)
                || (_scheduledTextures != null && _scheduledTextures.Count > 0);
        }

        public bool HasScheduledProperty(string propertyName)
        {
            if (_scheduledFloats != null && _scheduledFloats.ContainsKey(propertyName))
                return true;

            if (_scheduledInts != null && _scheduledInts.ContainsKey(propertyName))
                return true;

            if (_scheduledVector4s != null && _scheduledVector4s.ContainsKey(propertyName))
                return true;

            if (_scheduledColors != null && _scheduledColors.ContainsKey(propertyName))
                return true;

            if (_scheduledTextures != null && _scheduledTextures.ContainsKey(propertyName))
                return true;

            return false;
        }

        protected void schedule<T>(ref Dictionary<string, T> dict, string propertyName, T value)
        {
            if (dict == null)
                dict = new Dictionary<string, T>();

            addOrUpdateScheduled(dict, propertyName, value);
        }

        protected void addOrUpdateScheduled<T>(Dictionary<string, T> source, string propertyName, T value)
        {
            if (source.ContainsKey(propertyName))
                source[propertyName] = value;
            else
                source.Add(propertyName, value);
        }

        protected T getScheduled<T>(Dictionary<string, T> dict, string propertyName, T defaultValue)
        {
            if (dict == null)
                return defaultValue;

            if (dict.ContainsKey(propertyName))
                return dict[propertyName];

            return defaultValue;
        }

        protected float getProperty(string propertyName, float defaultValue = default)
        {
            if (_propertyBlock == null)
                return defaultValue;

            if (!hasFloatProperty(propertyName))
                return defaultValue;

            return _propertyBlock.GetFloat(propertyName);
        }

        protected int getProperty(string propertyName, int defaultValue = default)
        {
            if (_propertyBlock == null)
                return defaultValue;

            if (!hasIntProperty(propertyName))
                return defaultValue;

            return _propertyBlock.GetInt(propertyName);
        }

        protected Vector4 getProperty(string propertyName, Vector4 defaultValue = default)
        {
            if (_propertyBlock == null)
                return defaultValue;

            if (!hasVectorProperty(propertyName))
                return defaultValue;

            return _propertyBlock.GetVector(propertyName);
        }

        protected Color getProperty(string propertyName, Color defaultValue = default)
        {
            if (_propertyBlock == null)
                return defaultValue;

            if (!hasColorProperty(propertyName))
                return defaultValue;

            return _propertyBlock.GetColor(propertyName);
        }

        protected Texture getProperty(string propertyName, Texture defaultValue = default)
        {
            if (_propertyBlock == null)
                return defaultValue;

            if (!hasTextureProperty(propertyName))
                return defaultValue;

            return _propertyBlock.GetTexture(propertyName);
        }

        // In Unity version below 2021 there is no Has* API for Properties.
        // Thus we have to keep track of them ourselves.
#if !UNITY_2021_1_OR_NEWER
        protected List<string> _touchedProperties = new List<string>();
#endif

        protected bool hasFloatProperty(string propertyName)
        {
#if UNITY_2021_1_OR_NEWER
            return _propertyBlock.HasFloat(propertyName);
#else
            return _touchedProperties.Contains(propertyName);
#endif
        }

        protected bool hasIntProperty(string propertyName)
        {
#if UNITY_2021_1_OR_NEWER
            return _propertyBlock.HasInt(propertyName);
#else
            return _touchedProperties.Contains(propertyName);
#endif
        }

        protected bool hasVectorProperty(string propertyName)
        {
#if UNITY_2021_1_OR_NEWER
            return _propertyBlock.HasVector(propertyName);
#else
            return _touchedProperties.Contains(propertyName);
#endif
        }

        protected bool hasColorProperty(string propertyName)
        {
#if UNITY_2021_1_OR_NEWER
            return _propertyBlock.HasColor(propertyName);
#else
            return _touchedProperties.Contains(propertyName);
#endif
        }

        protected bool hasTextureProperty(string propertyName)
        {
#if UNITY_2021_1_OR_NEWER
            return _propertyBlock.HasTexture(propertyName);
#else
            return _touchedProperties.Contains(propertyName);
#endif
        }

        protected T get<T>(Dictionary<string, T> dict, string propertyName, System.Func<string, T, T> propertyGetter, T defaultValue = default)
        {
            if (dict != null && dict.ContainsKey(propertyName))
                return getScheduled(dict, propertyName, defaultValue);

            return propertyGetter.Invoke(propertyName, defaultValue);
        }

        /// <summary>
        /// Schedules a float change. Call Apply() to apply it.<br/>
        /// Notice that only the changes within one Schedule(..) -> Apply() cycle are applied, all other properties are reset.
        /// </summary>
        /// <param name="floatPropertyName"></param>
        /// <param name="value"></param>
        public void ScheduleFloat(string floatPropertyName, float value)
        {
            schedule(ref _scheduledFloats, floatPropertyName, value);
        }

        /// <summary>
        /// Returns the currently scheduled float. If none is set then defaultValue is returned.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public float GetScheduledFloat(string propertyName, float defaultValue = 0f)
        {
            return getScheduled(_scheduledFloats, propertyName, defaultValue);
        }

        /// <summary>
        /// Returns the currently set value of the PropertyBlock. If none is set then default(Float) is returned.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public float GetPropertyFloat(string propertyName, float defaultValue = 0f)
        {
            return getProperty(propertyName, defaultValue);
        }

        /// <summary>
        /// Returns the scheduled value and if no value is scheduled then returns the value of the property block.<br />
        /// If none is set then defaultValue is returned.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public float GetFloat(string propertyName, float defaultValue = 0f)
        {
            return get(_scheduledFloats, propertyName, GetPropertyFloat, defaultValue);
        }


        /// <summary>
        /// Schedules a int change. Call Apply() to apply it.<br/>
        /// Notice that only the changes within one Schedule(..) -> Apply() cycle are applied, all other properties are reset.
        /// </summary>
        /// <param name="intPropertyName"></param>
        /// <param name="value"></param>
        public void ScheduleInt(string propertyName, int value)
        {
            schedule(ref _scheduledInts, propertyName, value);
        }

        /// <summary>
        /// Returns the currently scheduled value. If none is set then defaultValue is returned.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetScheduledInt(string propertyName, int defaultValue = 0)
        {
            return getScheduled(_scheduledInts, propertyName, defaultValue);
        }

        /// <summary>
        /// Returns the currently set int of the PropertyBlock. If none is set then defaultValue is returned.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetPropertyInt(string propertyName, int defaultValue = 0)
        {
            return getProperty(propertyName, defaultValue);
        }

        /// <summary>
        /// Returns the scheduled value and if no value is scheduled then returns the value of the property block.<br />
        /// If none is set then defaultValue is returned.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public float GetInt(string propertyName, int defaultValue = 0)
        {
            return get(_scheduledInts, propertyName, GetPropertyInt, defaultValue);
        }

        /// <summary>
        /// Schedules a Vector4 change. Call Apply() to apply it.<br/>
        /// Notice that only the changes within one Schedule(..) -> Apply() cycle are applied, all other properties are reset.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void ScheduleVector4(string propertyName, Vector4 value)
        {
            schedule(ref _scheduledVector4s, propertyName, value);
        }

        /// <summary>
        /// Returns the currently scheduled value. If none is set then defaultValue is returned.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public Vector4 GetScheduledVector4(string propertyName, Vector4 defaultValue = default)
        {
            return getScheduled(_scheduledVector4s, propertyName, defaultValue);
        }

        /// <summary>
        /// Returns the currently set Vector4 of the PropertyBlock. If none is set then defaultValue is returned.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public Vector4 GetPropertyVector4(string propertyName, Vector4 defaultValue = default)
        {
            return getProperty(propertyName, defaultValue);
        }

        /// <summary>
        /// Returns the scheduled value and if no value is scheduled then returns the value of the property block.<br />
        /// If none is set then defaultValue is returned.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public Vector4 GetVector4(string propertyName, Vector4 defaultValue = default)
        {
            return get(_scheduledVector4s, propertyName, GetPropertyVector4, defaultValue);
        }

        /// <summary>
        /// Schedules a color change. Call Apply() to apply it.<br/>
        /// Notice that only the changes within one Schedule(..) -> Apply() cycle are applied, all other properties are reset.
        /// </summary>
        /// <param name="propertyName">An often used name is "_BaseColor" or "_Color".</param>
        /// <param name="color"></param>
        public void ScheduleColor(string propertyName, Color color)
        {
            schedule(ref _scheduledColors, propertyName, color);
        }

        /// <summary>
        /// Returns the currently scheduled color. If none is set then default(Color) is returned.
        /// </summary>
        /// <param name="colorPropertyName">An often used name is "_BaseColor".</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public Color GetScheduledColor(string propertyName, Color defaultValue = default)
        {
            return getScheduled(_scheduledColors, propertyName, defaultValue);
        }

        /// <summary>
        /// Returns the currently set color of the PropertyBlock. If none is set then default(Color) is returned.
        /// </summary>
        /// <param name="propertyName">An often used name is "_BaseColor".</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public Color GetPropertyColor(string propertyName, Color defaultValue = default)
        {
            return getProperty(propertyName, defaultValue);
        }

        /// <summary>
        /// Returns the scheduled color and if no color is scheduled then return the value of the property block.<br />
        /// If none is set then default(Color) is returned.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public Color GetColor(string propertyName, Color defaultValue = default)
        {
            return get(_scheduledColors, propertyName, GetPropertyColor, defaultValue);
        }

        /// <summary>
        /// Schedules a texture change. Call Apply() to apply it.<br/>
        /// Notice that only the changes within one Schedule(..) -> Apply() cycle are applied, all other properties are reset.
        /// </summary>
        /// <param name="propertyName">An often used name is "_MainTex".</param>
        /// <param name="texture"></param>
        public void ScheduleTexture(string propertyName, Texture texture)
        {
            schedule(ref _scheduledTextures, propertyName, texture);
        }

        /// <summary>
        /// Returns the currently scheduled value. If none is set then defaultValue is returned.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public Texture GetScheduledTexture(string propertyName, Texture defaultValue = default)
        {
            return getScheduled(_scheduledTextures, propertyName, defaultValue);
        }

        /// <summary>
        /// Returns the currently set texture of the PropertyBlock. If none is set then Null is returned.
        /// </summary>
        /// <param name="propertyName">Often used names are "_MainTex", "_SpecGlossMap", "_BumpMap", "_Occlusion".</param>
        /// <returns></returns>
        public Texture GetPropertyTexture(string propertyName, Texture defaultValue = default)
        {
            return getProperty(propertyName, defaultValue);
        }

        /// <summary>
        /// Returns the scheduled texture and if no texture change is scheduled then returns the texture of the property block.<br />
        /// If none is set then the defaultValue is returned.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public Texture GetTexture(string propertyName, Texture defaultValue = default)
        {
            return get(_scheduledTextures, propertyName, GetPropertyTexture, defaultValue);
        }

        protected void applyList<T>(Dictionary<string, T> dict, System.Action<string, T> setter)
        {
            if (dict != null)
            {
                foreach (var kv in dict)
                {
                    string propertyName = kv.Key;
                    setter.Invoke(propertyName, kv.Value);

#if !UNITY_2021_1_OR_NEWER
                    // Remember for Unity <= 2020
                    if (!_touchedProperties.Contains(propertyName))
                        _touchedProperties.Add(propertyName);
#endif
                }
            }
        }

        /// <summary>
        /// Applies all scheduled changes but does NOT clear the scheduled change list.
        /// </summary>
        public void Apply()
        {
            if (_propertyBlock == null)
                _propertyBlock = new MaterialPropertyBlock();

            if (Renderer == null)
                throw new System.Exception("Renderer is null.");

            // get block
            Renderer.GetPropertyBlock(_propertyBlock, MaterialIndex);

            applyList(_scheduledFloats, _propertyBlock.SetFloat);
            applyList(_scheduledInts, _propertyBlock.SetInt);
            applyList(_scheduledVector4s, _propertyBlock.SetVector);
            applyList(_scheduledColors, _propertyBlock.SetColor);

            // textures (need special handling)
            if (_scheduledTextures != null)
            {
                foreach (var kv in _scheduledTextures)
                {
                    var propertyName = kv.Key;
                    var value = kv.Value;
                    if (value != null)
                    {
                        _propertyBlock.SetTexture(propertyName, value);
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
                        _propertyBlock.SetTexture(propertyName, texture);
                    }

#if !UNITY_2021_1_OR_NEWER
                    // Remember for Unity <= 2020
                    if (!_touchedProperties.Contains(propertyName))
                        _touchedProperties.Add(propertyName);
#endif
                }
            }

            // set block
            Renderer.SetPropertyBlock(_propertyBlock, MaterialIndex);
        }

        /// <summary>
        /// Clears the cache which holds the scheduled changes.
        /// </summary>
        public void ClearScheduled()
        {
            if (_scheduledFloats != null) _scheduledFloats.Clear();
            if (_scheduledInts != null) _scheduledInts.Clear();
            if (_scheduledVector4s != null) _scheduledVector4s.Clear();
            if (_scheduledColors != null) _scheduledColors.Clear();
            if (_scheduledTextures != null) _scheduledTextures.Clear();
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

            _propertyBlock = new MaterialPropertyBlock();
            Renderer.SetPropertyBlock(_propertyBlock, MaterialIndex);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MaterialPropertyBlockSetter))]
    public class MaterialPropertyBlockSetterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var setter = target as MaterialPropertyBlockSetter;

            if (GUILayout.Button("Reset Properties"))
            {
                setter.ResetProperties();
                EditorUtils.RepaintViews();
            }

            if (GUILayout.Button("Test: Apply random color to _Color"))
            {
                setter.ScheduleColor("_Color", new Color(Random.value, Random.value, Random.value, Random.value));
                setter.Apply();
                EditorUtils.RepaintViews();
            }
        }
    }
#endif
}
