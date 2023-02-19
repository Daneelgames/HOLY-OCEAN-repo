using UnityEditor;
using UnityEngine;

namespace Kamgam.SettingsGenerator
{
    [CustomEditor(typeof(Settings))]
    public class SettingsEditor : Editor
    {
        public Settings settings;

        public void OnEnable()
        {
            settings = target as Settings;
        }

        override public void OnInspectorGUI()
        {
            serializedObject.Update();

            base.OnInspectorGUI();

            if(serializedObject.hasModifiedProperties)
            {
                EditorUtility.SetDirty(settings);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
