using JetBrains.Annotations;
using MrPink.Cheats;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace MrPink.Editor.Cheats
{
    public class CheatWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Cheats")]
        private static void OpenWindow()
        {
            var window = GetWindow<CheatWindow>();

            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
            window._config = AssetDatabase.LoadAssetAtPath<CheatConfig>("Assets/_src/Configs/Cheat Config.asset");
        }

        [SerializeField, InlineEditor(Expanded = true, DrawHeader = false)]
        private CheatConfig _config;



    }
}