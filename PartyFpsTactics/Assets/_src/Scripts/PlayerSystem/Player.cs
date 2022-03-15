using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class Player : MonoBehaviour
    {
        private static Player _instance;
        
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private PlayerMovement _movement;

        [SerializeField, ChildGameObjectsOnly, Required]
        private HealthController _health;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        public Camera _mainCamera;


        public static PlayerMovement Movement
            => _instance._movement;
        
        public static HealthController Health
            => _instance._health;

        public static GameObject GameObject
            => _instance.gameObject;
        
        public static Camera MainCamera
            => _instance._mainCamera;
        
        private void Awake()
        {
            _instance = this;
        }
    }
}