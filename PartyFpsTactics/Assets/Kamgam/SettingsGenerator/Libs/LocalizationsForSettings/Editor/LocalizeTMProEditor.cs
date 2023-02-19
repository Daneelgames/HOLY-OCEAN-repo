using UnityEditor;
using UnityEngine;

namespace Kamgam.LocalizationForSettings
{
    [CustomEditor(typeof(LocalizeTMPro))]
    public class LocalizeTMProEditor : Editor
    {
        public LocalizeTMPro localizer;

        protected SerializedProperty _providerProp;
        protected SerializedProperty _textfieldProp;
        protected SerializedProperty _termProp;
        protected SerializedProperty _updateFromTextProp;
        protected SerializedProperty _formatProp;

        protected string _lastText;

        public void OnEnable()
        {
            localizer = target as LocalizeTMPro;
            _providerProp = serializedObject.FindProperty("LocalizationProvider");
            _textfieldProp = serializedObject.FindProperty("Textfield");
            _termProp = serializedObject.FindProperty("Term");
            _updateFromTextProp = serializedObject.FindProperty("UpdateTermFromText");
            _formatProp = serializedObject.FindProperty("Format");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_providerProp);
            EditorGUILayout.PropertyField(_textfieldProp);


            if (localizer.UpdateTermFromText
                && localizer.Textfield != null
                && localizer.Textfield.text != _lastText
                && localizer.LocalizationProvider.HasLocalization()
                && localizer.LocalizationProvider.GetLocalization().HasTerm(localizer.Textfield.text.Trim()))
            {
                _termProp.stringValue = localizer.Textfield.text.Trim();
            }
            if (localizer.Textfield != null)
                _lastText = localizer.Textfield.text;

            // Draw term field in red is term is not found.
            bool foundTerm = LocalizationProvider.IsUsable(localizer.LocalizationProvider) && localizer.LocalizationProvider.Get(localizer.Term) != null;
            var bgColor = GUI.backgroundColor;
            GUI.backgroundColor = foundTerm ? Color.green : Color.red;
            EditorGUILayout.PropertyField(_termProp, new GUIContent("Term", "The term used for localization"));
            if (!foundTerm)
            {
                EditorGUILayout.HelpBox(new GUIContent("The term '" + _termProp.stringValue + "' has NOT been found in the localization table.\n" +
                    "It might be dynamic (then that's okay).\n" +
                    "But you might also have a typo in it (please check)."));
            }
            GUI.backgroundColor = bgColor;

            bool updateFromText = _updateFromTextProp.boolValue;
            EditorGUILayout.PropertyField(_updateFromTextProp);
            // Check if text machtes a term only if the checkbox was just enabled.
            if (_updateFromTextProp.boolValue && _updateFromTextProp.boolValue != updateFromText)
            {
                _lastText = null;
            }

            EditorGUILayout.PropertyField(_formatProp);

            if (serializedObject.hasModifiedProperties)
            {
                markAsChangedIfEditing();
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected void markAsChangedIfEditing()
        {
            if (EditorApplication.isPlaying)
                return;

            // Schedule an update to the scene view will rerender (otherwise
            // the change would not be visible unless clicked into the scene view).
            EditorApplication.QueuePlayerLoopUpdate();

            // Make sure the scene can be saved
            EditorUtility.SetDirty(localizer);

            // Make sure the Prefab recognizes the changes
            PrefabUtility.RecordPrefabInstancePropertyModifications(localizer);
        }
    }
}
