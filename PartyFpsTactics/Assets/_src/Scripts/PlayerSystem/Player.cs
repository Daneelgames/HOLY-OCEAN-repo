using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class Player : MonoBehaviour
    {
        private static Player _instance;
        
        
        [SerializeField, ChildGameObjectsOnly, Required]
        public Camera _mainCamera;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private HealthController _health;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private PlayerMovement _movement;

        [SerializeField, ChildGameObjectsOnly, Required]
        private CommanderControls _commanderControls;

        [SerializeField, ChildGameObjectsOnly, Required]
        private PlayerWeaponControls _weapon;
        [SerializeField, ChildGameObjectsOnly, Required]
        private PlayerThrowablesControls _throwableControls;

        [SerializeField, ChildGameObjectsOnly, Required]
        private PlayerInventory _inventory;
        [SerializeField, ChildGameObjectsOnly, Required]
        private PlayerInteractor _interactor;
        
        
        public static GameObject GameObject
            => _instance.gameObject;
        
        public static Camera MainCamera
            => _instance._mainCamera;

        public static PlayerMovement Movement
            => _instance._movement;
        
        public static HealthController Health
            => _instance._health;

        public static CommanderControls CommanderControls
            => _instance._commanderControls;

        public static PlayerWeaponControls Weapon
            => _instance._weapon;
        public static PlayerThrowablesControls ThrowableControls
            => _instance._throwableControls;

        public static PlayerInventory Inventory
            => _instance._inventory;
        public static PlayerInteractor Interactor
            => _instance._interactor;
        
        
        private void Awake()
        {
            _instance = this;
        }
    }
}