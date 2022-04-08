using System;
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
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private Transform _positionableObject;

        [SerializeField, ChildGameObjectsOnly, Required]
        private PlayerLookAround _lookAround;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private PlayerVehicleControls _vehicleControls;
        
        
        // FIXME дает слишком свободный доступ, к тому же объектов сейчас несколько
        public static GameObject GameObject
            => _instance._positionableObject.gameObject;
        
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

        public static Vector3 Position
            => _instance._positionableObject.position;


        public static PlayerLookAround LookAround
            => _instance._lookAround;
        public static PlayerVehicleControls VehicleControls
            => _instance._vehicleControls;
        

        public static void Death(Transform killer)
        {
            Movement.Death(killer);
            LookAround.Death(killer);
            Weapon.Death();
            ScoringSystem.Instance.CooldownToZero();
        }

        private void Awake()
        {
            _instance = this;
        }
    }
}