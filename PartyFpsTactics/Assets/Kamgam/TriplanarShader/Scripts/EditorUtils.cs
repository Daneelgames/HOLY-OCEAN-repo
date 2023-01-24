#if UNITY_EDITOR
using UnityEditor;

namespace Kamgam.TriplanarShader
{
    public static class EditorUtils
    {
        public static void RepaintViews()
        {
            System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
            System.Type type = assembly.GetType("UnityEditor.GameView");
            EditorWindow gameview = EditorWindow.GetWindow(type);
            gameview.Repaint();

            EditorWindow view = EditorWindow.GetWindow<SceneView>();
            view.Repaint();
        }
    }
}

#endif