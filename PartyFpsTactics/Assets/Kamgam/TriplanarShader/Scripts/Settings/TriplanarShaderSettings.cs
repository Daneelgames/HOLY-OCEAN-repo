#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kamgam.TriplanarShader
{
    // Create a new type of Settings Asset.
    public class TriplanarShaderSettings : ScriptableObject
    {
        public const string Version = "1.0.0";
        public const string SettingsFilePath = "Assets/TriplanarShaderSettings.asset";

        [SerializeField, Tooltip(_logLevelTooltip)]
        public Logger.LogLevel LogLevel;
        public const string _logLevelTooltip = "Any log above this log level will not be shown. To turn off all logs choose 'NoLogs'";

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

        static TriplanarShaderSettings cachedSettings;

        public static TriplanarShaderSettings GetOrCreateSettings()
        {
            if (cachedSettings == null)
            {
                string typeName = typeof(TriplanarShaderSettings).Name;

                cachedSettings = AssetDatabase.LoadAssetAtPath<TriplanarShaderSettings>(SettingsFilePath);

                // Still not found? Then search for it.
                if (cachedSettings == null)
                {
                    string[] results = AssetDatabase.FindAssets("t:" + typeName);
                    if (results.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(results[0]);
                        cachedSettings = AssetDatabase.LoadAssetAtPath<TriplanarShaderSettings>(path);
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

                    cachedSettings = ScriptableObject.CreateInstance<TriplanarShaderSettings>();
                    cachedSettings.LogLevel = Logger.LogLevel.Log;

                    AssetDatabase.CreateAsset(cachedSettings, SettingsFilePath);
                    AssetDatabase.SaveAssets();

                    // Import packages and then show welcome screen.
                    bool importNeeded = true;
                    if(PackageImporter.GetCurrentRenderPiplelineType() == PackageImporter.RenderPiplelineType.URP)
                    {
                        importNeeded = !System.IO.Directory.Exists("Assets/Kamgam/TriplanarShader/URP");
                    }
                    if (PackageImporter.GetCurrentRenderPiplelineType() == PackageImporter.RenderPiplelineType.HDRP)
                    {
                        importNeeded = !System.IO.Directory.Exists("Assets/Kamgam/TriplanarShader/HDRP");
                    }

                    if (importNeeded)
                    {
                        PackageImporter.ImportDelayed(onSettingsCreated);
                    }
                    else
                    {
                        onSettingsCreated();
                    }
                }
            }

            return cachedSettings;
        }

        private static void onCompilationStarted(object obj)
        {
            string typeName = typeof(TriplanarShaderSettings).Name;
            SessionState.SetBool(typeName + "WaitingForReload", true);
        }

        // We use this callback instead of CompilationPipeline.compilationFinished because
        // compilationFinished runs before the assemply has been reloaded but DidReloadScripts
        // runs after. And only after we can access the Settings asset.
        [UnityEditor.Callbacks.DidReloadScripts(999000)]
        public static void DidReloadScripts()
        {
            string typeName = typeof(TriplanarShaderSettings).Name;
            SessionState.EraseBool(typeName + "WaitingForReload");
        }

        static void onSettingsCreated()
        {
            bool openDemo = EditorUtility.DisplayDialog(
                    "Triplanar Shader",
                    "Thank you for choosing Triplanar Shader.\n\n" +
                    "Please start by reading the manual.\n\n" +
                    "It would be great if you could find the time to leave a review.",
                    "Open Demo Scene", "Open manual"
                    );

            if (openDemo)
            {
                OpenExample();
			}
            else
            {
                OpenManual();
            }
        }

        [MenuItem("Tools/Triplanar Shader/Manual", priority = 101)]
        public static void OpenManual()
        {
            Application.OpenURL("https://kamgam.com/unity/TriplanarShaderManual.pdf");
        }

        [MenuItem("Tools/Triplanar Shader/Open Demo Scene", priority = 103)]
        public static void OpenExample()
        {
            string renderPipelineFolder = "BuiltIn";
            var rp = PackageImporter.GetCurrentRenderPiplelineType();
            if (rp == PackageImporter.RenderPiplelineType.URP)
                renderPipelineFolder = "URP";
            else if (rp == PackageImporter.RenderPiplelineType.HDRP)
                renderPipelineFolder = "HDRP";

            string path = "Assets/Kamgam/TriplanarShader/"+ renderPipelineFolder + "/Demo/Scripting/DemoScripting.unity";
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            EditorGUIUtility.PingObject(scene);
            EditorSceneManager.OpenScene(path);
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        [MenuItem("Tools/Triplanar Shader/Settings", priority = 100)]
        public static void OpenSettings()
        {
            var settings = TriplanarShaderSettings.GetOrCreateSettings();
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Triplanar Shader Settings could not be found or created.", "Ok");
            }
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }

    static class TriplanarShaderSettingsProvider
    {
        [SettingsProvider]
        public static UnityEditor.SettingsProvider CreateTriplanarShaderSettingsProvider()
        {
            var provider = new UnityEditor.SettingsProvider("Project/Triplanar Shader", SettingsScope.Project)
            {
                label = "Triplanar Shader",
                guiHandler = (searchContext) =>
                {
                    var settings = TriplanarShaderSettings.GetSerializedSettings();

                    var style = new GUIStyle(GUI.skin.label);
                    style.wordWrap = true;

                    EditorGUILayout.LabelField("Version: " + TriplanarShaderSettings.Version);

                    if (drawButton(" Open Manual ", icon: "_Help"))
                    {
                        TriplanarShaderSettings.OpenManual();
                    }

                    settings.ApplyModifiedProperties();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting.
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "shader", "triplanar", "rendering" })
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