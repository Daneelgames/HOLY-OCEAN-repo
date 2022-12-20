using System;
using System.Collections;
using FishNet.Connection;
using FishNet.Object;
using MrPink.Health;
using MrPink.Units;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class Player : NetworkBehaviour
    {
        [SerializeField, Required]
        public Camera _mainCamera;
        
        [SerializeField, Required]
        private HealthController _health;
        
        [SerializeField, Required]
        private PlayerMovement _movement;

        
        [SerializeField, Required]
        private CommanderControls _commanderControls;

        [SerializeField, Required]
        private PlayerWeaponControls _weapon;
        
        [SerializeField, Required]
        private PlayerToolsControls toolControls;

        [SerializeField, Required]
        private PlayerInventory _inventory;
        
        [SerializeField, Required]
        private PlayerInteractor _interactor;
        

        [SerializeField, Required]
        private PlayerLookAround _lookAround;
        
        [SerializeField, Required]
        private PlayerVehicleControls _vehicleControls;
        [SerializeField, Required]
        private CharacterNeeds _characterNeeds;
        
        
        // FIXME дает слишком свободный доступ, к тому же объектов сейчас несколько
        public GameObject GameObject
            => gameObject;
        
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
        public PlayerToolsControls ToolControls
            => toolControls;

        public PlayerInventory Inventory
            => _inventory;
        
        public PlayerInteractor Interactor
            => _interactor;

        public Vector3 Position
            => transform.position;


        public PlayerLookAround LookAround
            => _lookAround;
        public PlayerVehicleControls VehicleControls
            => _vehicleControls;
        public CharacterNeeds CharacterNeeds
            => _characterNeeds;

        public override void OnStartClient() { 
            base.OnStartClient();
          
                Init();
        }
        private void Init()
        {
            Game._instance.AddPlayer(this);
            _health.SetIsPlayerTrue();
        }
        
        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);
            /* Current owner can be found by using base.Owner. prevOwner
            * contains the connection which lost ownership. Value will be
            * -1 if there was no previous owner. */
            
            SetLocalPlayerInstance();
        }

        [Client(RequireOwnership = true)]
        void SetLocalPlayerInstance()
        {
            Debug.Log(gameObject.name + " PLAYER PUT HIMSELF AS LOCAL PLAYER");
            
            if (base.IsHost == false)
            {
                SceneLoader.Instance.gameObject.SetActive(false);
            }
            
            
            Game._instance.SetLocalPlayer(this);
        }

        public void Death(Transform killer)
        {
            Game.LocalPlayer.Interactor.SetInteractionText("R TO RESTART");
            Movement.Death(killer);
            LookAround.Death(killer);
            Weapon.Death();
            VehicleControls.Death();
            ScoringSystem.Instance.CooldownToZero();
        }
        
        public void Resurrect()
        {
            Game.LocalPlayer.Inventory.DropAll();
            Game.LocalPlayer.Interactor.SetInteractionText("");
            Health.Resurrect();
            Movement.Resurrect();
            LookAround.Resurrect();
            Weapon.Resurrect();
            CharacterNeeds.ResetNeeds();
        }

        void OnDestroy()
        {
            Game._instance.RemovePlayer(this);
        }
    }
}