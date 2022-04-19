using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.Cheats
{
    [CreateAssetMenu(fileName = "Cheat Config", menuName = "MrPink/Cheat")]
    public class CheatConfig : ScriptableObject
    {
        [ShowInInspector]
        [OnValueChanged(nameof(SetImmortalState))]
        private bool _isImmortal;
        
        [ShowInInspector]
        [OnValueChanged(nameof(SetMuteState))]
        private bool _isMuted;
        
        
        public void ApplyAll()
        {
            SetImmortalState(_isImmortal);
            SetMuteState(_isMuted);
        }
        
        private static void SetImmortalState(bool value)
        {
            if (!Application.isPlaying)
                return;
            
            var status = value ? "activated" : "deactivated";
            Debug.Log($"Immortality cheat {status}");

            Game.Player.Health.IsImmortal = value;
        }

        private static void SetMuteState(bool value)
        {
            if (!Application.isPlaying)
                return;
            
            Debug.Log(value ? "Muted" : "Unmuted");

            AudioListener.volume = value ? 0 : 1;
        }
    }
}