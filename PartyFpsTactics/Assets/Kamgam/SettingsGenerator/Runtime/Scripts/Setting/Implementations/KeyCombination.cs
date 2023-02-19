using Kamgam.UGUIComponentsForSettings;

namespace Kamgam.SettingsGenerator
{
    [System.Serializable]
    public struct KeyCombination
    {
        public UniversalKeyCode Key;

        /// <summary>
        /// Used for modifier keys (CTRL, ALT, SHIFT, TAB) in key combinations.
        /// </summary>
        public UniversalKeyCode ModifierKey;

        public KeyCombination(UniversalKeyCode key)
        {
            Key = key;
            ModifierKey = UniversalKeyCode.None;
        }

        public KeyCombination(UniversalKeyCode key, UniversalKeyCode modifierKey)
        {
            Key = key;
            ModifierKey = modifierKey;
        }

        public bool Equals(KeyCombination combination)
        {
            return Key == combination.Key && ModifierKey == combination.ModifierKey;
        }
    }
}
