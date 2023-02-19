using UnityEditor;
using UnityEngine;

namespace Kamgam.SettingsGenerator
{
    [CustomPropertyDrawer(typeof(ISetting), true)]
    public class SettingProperyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, property.isExpanded);
        }
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            // Replace label with ID.
            try
            {
                string id = property.FindPropertyRelative("ID").stringValue;
                if (string.IsNullOrEmpty(id))
                    id = "(New Setting)";

                label = new GUIContent(id);
            }
            catch
            {
                // keep default label
            }

            EditorGUI.PropertyField(rect, property, label, property.isExpanded);
        }
    }
}
