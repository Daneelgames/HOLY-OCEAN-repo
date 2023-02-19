using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kamgam.UGUIComponentsForSettings
{
    [CustomEditor(typeof(StepperUGUI))]
    public class StepperUGUIEditor : Editor
    {
        public StepperUGUI stepper;

        public void OnEnable()
        {
            stepper = target as StepperUGUI;
        }

        override public void OnInspectorGUI()
        {
            var oldText = stepper.Text;
            stepper.Text = EditorGUILayout.TextField("Text:", stepper.Text);
            if(oldText != stepper.Text)
            {
                markAsChangedIfEditing();
            }

            var oldValue = stepper.Value;
            stepper.Value = EditorGUILayout.Slider("Value:", stepper.Value, stepper.MinValue, stepper.MaxValue);
            if (oldValue != stepper.Value)
            {
                markAsChangedIfEditing();
            }

            if (stepper.WholeNumbers)
            {
                stepper.MinValue = Mathf.Round(stepper.MinValue);
                stepper.MaxValue = Mathf.Round(stepper.MaxValue);
            }

            base.OnInspectorGUI();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Decrease (-)"))
            {
                stepper.Decrease();
                markAsChangedIfEditing();
            }

            if (GUILayout.Button("Increase (+)"))
            {
                stepper.Increase();
                markAsChangedIfEditing();
            }

            EditorGUILayout.EndHorizontal();
        }

        protected void markAsChangedIfEditing()
        {
            if (EditorApplication.isPlaying)
                return;

            // Schedule an update to the scene view will rerender (otherwise
            // the change would not be visible unless clicked into the scene view).
            EditorApplication.QueuePlayerLoopUpdate();

            // Make sure the scene can be saved
            EditorUtility.SetDirty(stepper);

            // Make sure the Prefab recognizes the changes
            PrefabUtility.RecordPrefabInstancePropertyModifications(stepper);
            PrefabUtility.RecordPrefabInstancePropertyModifications(stepper.TextTf);
            PrefabUtility.RecordPrefabInstancePropertyModifications(stepper.ValueTf);
        }
    }
}
