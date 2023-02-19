using Kamgam.LocalizationForSettings;
using UnityEditor;
using UnityEngine;

namespace Kamgam.SettingsGenerator
{
    [CustomEditor(typeof(SettingResolver), true)]
    public class SettingResolverEditor : Editor
    {
        SerializedProperty _settingProviderProp;
        SerializedProperty _localizationProviderProp;
        SerializedProperty _idProp;

        public SettingResolver resolver;
        public string[] availableIDs;
        public SettingsProvider availableIDsFromProvider;
        protected Vector2 scrollViewPos;

        public void OnEnable()
        {
            _idProp = serializedObject.FindProperty("ID");
            _settingProviderProp = serializedObject.FindProperty("SettingsProvider");
            _localizationProviderProp = serializedObject.FindProperty("LocalizationProvider");

            resolver = target as SettingResolver;

            // Auto select if settings provider is null
            string[] providerGUIDs;
            if (resolver.SettingsProvider == null)
            {
                providerGUIDs = AssetDatabase.FindAssets("t:" + typeof(SettingsProvider).Name);
                if (providerGUIDs.Length > 0)
                {
                    resolver.SettingsProvider = AssetDatabase.LoadAssetAtPath<SettingsProvider>(AssetDatabase.GUIDToAssetPath(providerGUIDs[0]));
                    markAsChangedIfEditing();
                }
            }

            // Auto select if localization provider is null
            if (resolver.LocalizationProvider == null)
            {
                providerGUIDs = AssetDatabase.FindAssets("t:" + typeof(LocalizationProvider).Name);
                if (providerGUIDs.Length > 0)
                {
                    resolver.LocalizationProvider = AssetDatabase.LoadAssetAtPath<LocalizationProvider>(AssetDatabase.GUIDToAssetPath(providerGUIDs[0]));
                    markAsChangedIfEditing();
                }
            }

            updateAvailableIDs();
        }

        private void updateAvailableIDs()
        {
            // Fetch available setting ids from settings provider
            if (resolver.SettingsProvider != null && resolver.SettingsProvider.SettingsAsset != null)
            {
                availableIDs = resolver.SettingsProvider.SettingsAsset.GetSettingIDsOrderedByName(true, resolver.GetSupportedDataTypes());
                availableIDsFromProvider = resolver.SettingsProvider;
            }
        }

        public override void OnInspectorGUI()
        {
            Color tmpColor, tmpBgColor;

            serializedObject.Update();

            // Provider field
            EditorGUILayout.PropertyField(_settingProviderProp);

            EditorGUILayout.PropertyField(_localizationProviderProp);

            // ID field (red if not set)
            tmpColor = GUI.color;
            tmpBgColor = GUI.backgroundColor;
            if (string.IsNullOrEmpty(resolver.ID)) {
                GUI.backgroundColor = Color.red;
                GUI.color = Color.red;
            }
            GUI.SetNextControlName("idInput");
            EditorGUILayout.PropertyField(_idProp);
            GUI.color = tmpColor;
            GUI.backgroundColor = tmpBgColor;

            // Force update if provider changed.
            if(resolver != null && resolver.SettingsProvider != availableIDsFromProvider)
            {
                updateAvailableIDs();
            }

            // Warnings and ID suggestions
            if (resolver.SettingsProvider != null)
            {
                // Warnings
                if(resolver.SettingsProvider.SettingsAsset != null)
                {
                    var setting = resolver.SettingsProvider.SettingsAsset.GetSetting(resolver.ID);
                    if (setting != null)
                    {
                        if (setting.HasConnectionObject())
                        {
                            GUILayout.Label(new GUIContent("This Setting uses a CONNECTION.", "The setting of this resolver is using a Connection and is filled dynamically.\nThis means that any options you add in the UI will be ignored."));
                        }
                        else
                        {
                            if(setting is SettingOption settingWithOptions && settingWithOptions.HasOptions())
                            {
                                GUILayout.Label(new GUIContent("This Setting has static OPTIONS.", "The setting of this resolver has some static options defined.\nThis means that any options you add in the UI will be ignored."));
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(resolver.ID))
                    {
                        tmpColor = GUI.color;
                        GUI.color = Color.red;
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(new GUIContent("Please set an ID.", "This resolver has no ID set. It will do nothing as it will not be able to find any setting."));
                        GUILayout.EndHorizontal();
                        GUI.color = tmpColor;
                    }
                }

                // ID Suggenstions
                if (availableIDs != null)
                {
                    GUILayout.Label(new GUIContent("Suggested IDs:", "These are IDs of settings within your 'DefaultSettings.asset'." +
                        "\n\nIt lists only IDs which match the supported data type of this resolver."));

                    scrollViewPos = GUILayout.BeginScrollView(scrollViewPos, GUILayout.MaxHeight(100));
                    string idLower = resolver.ID == null ? "" : resolver.ID.Trim().ToLower();
                    string firstCandidate = null;
                    int shownIDs = 0;
                    for (int i = 0; i < availableIDs.Length; i++)
                    {
                        if (availableIDs[i].ToLower().StartsWith(idLower))
                        {
                            shownIDs++;
                            GUI.enabled = availableIDs[i] != resolver.ID;

                            if (firstCandidate == null && GUI.enabled)
                                firstCandidate = availableIDs[i];

                            if (GUILayout.Button(availableIDs[i]))
                            {
                                resolver.ID = availableIDs[i];
                                _idProp.stringValue = resolver.ID;
                                GUI.FocusControl(null);
                                markAsChangedIfEditing();
                            }

                            GUI.enabled = true;
                        }
                    }
                    // Show containing as fallback
                    if (shownIDs == 0)
                    {
                        for (int i = 0; i < availableIDs.Length; i++)
                        {
                            if (availableIDs[i].ToLower().Contains(idLower))
                            {
                                shownIDs++;
                                GUI.enabled = availableIDs[i] != resolver.ID;

                                if (firstCandidate == null && GUI.enabled)
                                    firstCandidate = availableIDs[i];

                                if (GUILayout.Button(availableIDs[i]))
                                {
                                    resolver.ID = availableIDs[i];
                                    _idProp.stringValue = resolver.ID;
                                    GUI.FocusControl(null);
                                    markAsChangedIfEditing();
                                }

                                GUI.enabled = true;
                            }
                        }
                    }
                    GUILayout.EndScrollView();

                    // apply first candidate if the down arrow is pressed
                    if (GUI.GetNameOfFocusedControl() == "idInput" && Event.current.keyCode == KeyCode.DownArrow && firstCandidate != null && resolver.ID != firstCandidate)
                    {
                        resolver.ID = firstCandidate;
                        _idProp.stringValue = resolver.ID;
                        GUI.FocusControl(null);
                        markAsChangedIfEditing();
                    }

                    if (shownIDs == 0)
                    {
                        tmpColor = GUI.color;
                        GUI.color = new Color(1f, 1f, 0.2f);
                        GUILayout.Label(new GUIContent("No machting IDs found! Is it a dynamic setting?", "Resolving this settings will fail unless a setting with this id is added dynamically via code."));
                        GUI.color = tmpColor;
                    }
                }
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
            EditorUtility.SetDirty(resolver);

            // Make sure the Prefab recognizes the changes
            PrefabUtility.RecordPrefabInstancePropertyModifications(resolver);
        }
    }
}
