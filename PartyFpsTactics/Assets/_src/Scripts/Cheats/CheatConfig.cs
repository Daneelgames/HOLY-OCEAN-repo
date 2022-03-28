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
        
        
        public void ApplyAll()
        {
            SetImmortalState(_isImmortal);
        }
        
        private static void SetImmortalState(bool value)
        {
            if (!Application.isPlaying)
                return;
            
            var status = value ? "activated" : "deactivated";
            Debug.Log($"Immortality cheat {status}");

            Player.Health.IsImmortal = value;
        }
    }
}