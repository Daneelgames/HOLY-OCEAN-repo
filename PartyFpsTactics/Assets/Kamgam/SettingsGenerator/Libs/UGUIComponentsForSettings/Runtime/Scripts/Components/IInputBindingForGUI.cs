namespace Kamgam.UGUIComponentsForSettings
{
    /// <summary>
    /// Interface for input binding GUI. Use this to add your own input binding.
    /// The default implementation assumes the Unity InputSystem is used.
    /// </summary>
    public interface IInputBindingForGUI
    {
        public string GetBindingPath();
        public void SetBindingPath(string path);

        public void StartListening();
        public void AddOnCompleteCallback(System.Action callback);
        public void RemoveOnCompleteCallback(System.Action callback);
        public void AddOnCanceledCallback(System.Action callback);
        public void RemoveOnCanceledCallback(System.Action callback);

        public void OnEnable();
        public void OnDisable();
    }
}
