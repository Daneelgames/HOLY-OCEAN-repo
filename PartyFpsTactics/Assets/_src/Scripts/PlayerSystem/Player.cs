using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class Player : MonoBehaviour
    {
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
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private Transform _positionableObject;

        [SerializeField, ChildGameObjectsOnly, Required]
        private PlayerLookAround _lookAround;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private PlayerVehicleControls _vehicleControls;
        
        
        // FIXME дает слишком свободный доступ, к тому же объектов сейчас несколько
        public GameObject GameObject
            => _positionableObject.gameObject;
        
        public Camera MainCamera
            => _mainCamera;

        public PlayerMovement Movement
            => _movement;
        
        public HealthController Health
            => _health;

        public CommanderControls CommanderControls
            => _commanderControls;

        public PlayerWeaponControls Weapon
            => _weapon;
        public PlayerThrowablesControls ThrowableControls
            => _throwableControls;

        public PlayerInventory Inventory
            => _inventory;
        
        public PlayerInteractor Interactor
            => _interactor;

        public Vector3 Position
            => _positionableObject.position;


        public PlayerLookAround LookAround
            => _lookAround;
        public PlayerVehicleControls VehicleControls
            => _vehicleControls;
        

        public void Death(Transform killer)
        {
            Movement.Death(killer);
            LookAround.Death(killer);
            Weapon.Death();
            ScoringSystem.Instance.CooldownToZero();
        }

    }
}