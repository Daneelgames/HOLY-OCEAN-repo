using UnityEngine;

namespace Kamgam.SettingsGenerator
{
    public class AudioSourceVolumeConnectionComponent : MonoBehaviour
    {
        public SettingsProvider SettingsProvider;
        public string ID;

        [Tooltip("How the input should be mapped to the requierd ouput of 0f..1f (X = min, Y = max).\n" +
            "Useful if you have a range in percent (from 0 to 100) but need output ranging from 0f to 1f.")]
        public Vector2 InputRange = new Vector2(0f, 100f);

        public AudioSource[] AudioSources;

        [System.NonSerialized]
        public AudioSourceVolumeConnection Connection;

        public void Start()
        {
            Connection = new AudioSourceVolumeConnection(InputRange, AudioSources);
            SettingsProvider.Settings.GetOrCreateFloat(ID, Connection.Get(), connection: Connection);
        }

        public void Reset()
        {
            AudioSources = GetComponents<AudioSource>();

#if UNITY_EDITOR
            // Auto select if localization provider is null
            if (SettingsProvider == null)
            {
                var providerGUIDs = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(SettingsProvider).Name);
                if (providerGUIDs.Length > 0)
                {
                    SettingsProvider = UnityEditor.AssetDatabase.LoadAssetAtPath<SettingsProvider>(UnityEditor.AssetDatabase.GUIDToAssetPath(providerGUIDs[0]));
                    markAsChangedIfEditing();
                }
            }
#endif
        }

#if UNITY_EDITOR
        protected void markAsChangedIfEditing()
        {
            if (UnityEditor.EditorApplication.isPlaying)
                return;

            // Schedule an update to the scene view will rerender (otherwise
            // the change would not be visible unless clicked into the scene view).
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();

            // Make sure the scene can be saved
            UnityEditor.EditorUtility.SetDirty(this);

            // Make sure the Prefab recognizes the changes
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
        }
#endif

    }
}
