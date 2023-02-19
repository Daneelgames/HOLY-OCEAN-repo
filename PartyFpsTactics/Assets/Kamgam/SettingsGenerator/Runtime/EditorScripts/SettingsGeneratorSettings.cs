#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kamgam.SettingsGenerator
{
    // Create a new type of Settings Asset.
    public class SettingsGeneratorSettings : ScriptableObject
    {
        public const string Version = "1.4.1";
        public const string SettingsFilePath = "Assets/SettingsGeneratorSettings.asset";

        [SerializeField, Tooltip(_logLevelTooltip)]
        public Logger.LogLevel LogLevel = Logger.LogLevel.Log;
        public const string _logLevelTooltip = "Any log above this log level will not be shown. To turn off all logs choose 'NoLogs'";

        [SerializeField, Tooltip(_showEditorInfoLogsTooltip)]
        public bool ShowEditorInfoLogs = true;
        public const string _showEditorInfoLogsTooltip = "Turn off if you no longer want to see the 'Setting has no effect in the Editor. Please try in a build.' log messages.";
        public const string _showEditorInfoLogsHint = "You can turn this log message off in the settings (Tools > Settings Generator > Settings : Show Editor Info Logs).";

        [RuntimeInitializeOnLoadMethod]
        static void bindLoggerLevelToSetting()
        {
            // Notice: This does not yet create a setting instance!
            Logger.OnGetLogLevel = () => GetOrCreateSettings().LogLevel;
        }

        [InitializeOnLoadMethod]
        static void autoCreateSettings()
        {
            GetOrCreateSettings();
        }

        static SettingsGeneratorSettings cachedSettings;

        public static SettingsGeneratorSettings GetOrCreateSettings()
        {
            if (cachedSettings == null)
            {
                string typeName = typeof(SettingsGeneratorSettings).Name;

                cachedSettings = AssetDatabase.LoadAssetAtPath<SettingsGeneratorSettings>(SettingsFilePath);

                // Still not found? Then search for it.
                if (cachedSettings == null)
                {
                    string[] results = AssetDatabase.FindAssets("t:" + typeName);
                    if (results.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(results[0]);
                        cachedSettings = AssetDatabase.LoadAssetAtPath<SettingsGeneratorSettings>(path);
                        cachedSettings.ShowEditorInfoLogs = true;
                        cachedSettings.LogLevel = Logger.LogLevel.Log;
                    }
                }

                if (cachedSettings != null)
                {
                    SessionState.EraseBool(typeName + "WaitingForReload");
                }

                // Still not found? Then create settings.
                if (cachedSettings == null)
                {
                    CompilationPipeline.compilationStarted -= onCompilationStarted;
                    CompilationPipeline.compilationStarted += onCompilationStarted;

                    // Are the settings waiting for a recompile to finish? If yes then return null;
                    // This is important if an external script tries to access the settings before they
                    // are deserialized after a re-compile.
                    bool isWaitingForReloadAfterCompilation = SessionState.GetBool(typeName + "WaitingForReload", false);
                    if (isWaitingForReloadAfterCompilation)
                    {
                        Debug.LogWarning(typeName + " is waiting for assembly reload.");
                        return null;
                    }

                    cachedSettings = ScriptableObject.CreateInstance<SettingsGeneratorSettings>();
                    cachedSettings.LogLevel = Logger.LogLevel.Log;

                    AssetDatabase.CreateAsset(cachedSettings, SettingsFilePath);
                    AssetDatabase.SaveAssets(); 

                    // Import packages and then show welcome screen.
                    PackageImporter.ImportDelayed(onSettingsCreated);
                }
            }

            return cachedSettings;
        }

        private static void onCompilationStarted(object obj)
        {
            string typeName = typeof(SettingsGeneratorSettings).Name;
            SessionState.SetBool(typeName + "WaitingForReload", true);
        }

        // We use this callback instead of CompilationPipeline.compilationFinished because
        // compilationFinished runs before the assemply has been reloaded but DidReloadScripts
        // runs after. And only after we can access the Settings asset.
        [UnityEditor.Callbacks.DidReloadScripts(999000)]
        public static void DidReloadScripts()
        {
            string typeName = typeof(SettingsGeneratorSettings).Name;
            SessionState.EraseBool(typeName + "WaitingForReload");
        }

        static void onSettingsCreated()
        {
            bool openExample = EditorUtility.DisplayDialog(
                    "Settings Generator",
                    "Thank you for choosing Settings Generator.\n\n" +
                    "Please start by reading the manual.\n\n" +
                    "If you can find the time I would appreciate your feedback in the form of a review.\n\n" +
                    "I have prepared some examples for you.",
                    "Open Example", "Open manual (web)"
                    );

            if (openExample)
                OpenExample();
            else
                OpenManual();
        }

        [MenuItem("Tools/Settings Generator/Manual", priority = 101)]
        public static void OpenManual()
        {
            Application.OpenURL("https://kamgam.com/unity/SettingsGeneratorManual.pdf");
        }

        [MenuItem("Tools/Settings Generator/Open Example Scene", priority = 103)]
        public static void OpenExample()
        {
            string path = "Assets/Kamgam/SettingsGenerator/Examples/FromAsset/SettingsFromAssetDemo.unity";
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            EditorGUIUtility.PingObject(scene);
            EditorSceneManager.OpenScene(path);
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        [MenuItem("Tools/Settings Generator/Settings", priority = 100)]
        public static void OpenSettings()
        {
            var settings = SettingsGeneratorSettings.GetOrCreateSettings();
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Settings Generator Settings could not be found or created.", "Ok");
            }
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SettingsGeneratorSettings))]
    public class SettingsGeneratorSettingsEditor : Editor
    {
        public SettingsGeneratorSettings settings;

        public void OnEnable()
        {
            settings = target as SettingsGeneratorSettings;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Version: " + SettingsGeneratorSettings.Version);
            base.OnInspectorGUI();
        }
    }
#endif

    static class SettingsGeneratorSettingsProvider
    {
        [SettingsProvider]
        public static UnityEditor.SettingsProvider CreateSettingsGeneratorSettingsProvider()
        {
            var provider = new UnityEditor.SettingsProvider("Project/Settings Generator", SettingsScope.Project)
            {
                label = "Settings Generator",
                guiHandler = (searchContext) =>
                {
                    var settings = SettingsGeneratorSettings.GetSerializedSettings();

                    var style = new GUIStyle(GUI.skin.label);
                    style.wordWrap = true;

                    EditorGUILayout.LabelField("Version: " + SettingsGeneratorSettings.Version);
                    drawField("LogLevel", "Log Level", SettingsGeneratorSettings._logLevelTooltip, settings, style);
                    drawField("ShowEditorInfoLogs", "Show Editor Info Logs", SettingsGeneratorSettings._showEditorInfoLogsTooltip, settings, style);

                    if (drawButton(" Open Manual ", icon: "_Help"))
                    {
                        SettingsGeneratorSettings.OpenManual();
                    }

                    settings.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting.
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "settings", "options", "ui", "gui", "ugui", "generator", "creator", "uss", "unified", "controls" })
            };

            return provider;
        }

        static void drawField(string propertyName, string label, string tooltip, SerializedObject settings, GUIStyle style)
        {
            EditorGUILayout.PropertyField(settings.FindProperty(propertyName), new GUIContent(label));
            if (!string.IsNullOrEmpty(tooltip))
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Label(tooltip, style);
                GUILayout.EndVertical();
            }
            GUILayout.Space(10);
        }

        static bool drawButton(string text, string tooltip = null, string icon = null, params GUILayoutOption[] options)
        {
            GUIContent content;

            // icon
            if (!string.IsNullOrEmpty(icon))
                content = EditorGUIUtility.IconContent(icon);
            else
                content = new GUIContent();

            // text
            content.text = text;

            // tooltip
            if (!string.IsNullOrEmpty(tooltip))
                content.tooltip = tooltip;

            return GUILayout.Button(content, options);
        }
    }
}
#endif