using System;
using Brezg.Serialization;
using MrPink.Health;
using MrPink.WeaponsSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class PlayerWeaponControls : MonoBehaviour
    {
        [SerializeField, SceneObjectsOnly, Required]
        private Transform _weaponsTargetsParent;
        
        [SerializeField, SceneObjectsOnly, Required]
        private Transform _weaponsParent;

        [SerializeField, ChildGameObjectsOnly, Required]
        private UnityDictionary<Hand, WeaponHand> _hands = new UnityDictionary<Hand, WeaponHand>();

        public UnityDictionary<Hand, WeaponHand> Hands => _hands;
        
        
        [Header("CAMERA")]
        public float camFovIdle = 90;
        public float camFovAim = 90;
        public float fovChangeSpeed = 90;
        

        float targetFov = 90;
        
        private HealthController localPlayerHealth;

        private bool _isDead
        {
            get
            {
                if (localPlayerHealth)
                    return localPlayerHealth.health <= 0;

                localPlayerHealth = gameObject.GetComponent<HealthController>();
                return localPlayerHealth.health <= 0;
            }
        }

        private void OnEnable()
        {
            _weaponsParent.parent = Game._instance.PlayerCamera.transform;
            _weaponsParent.localPosition = Vector3.zero;
            _weaponsParent.localRotation = Quaternion.identity;
        }

        private void Update()
        {
            if (Game.LocalPlayer == null) return;
            
            if (Shop.Instance && Shop.Instance.IsActive)
                return;
            if (PlayerInventoryUI.Instance && PlayerInventoryUI.Instance.IsActive)
                return;
            
            if (Game.Flags.IsPlayerInputBlocked || Game.LocalPlayer.Interactor.carryingPortableRb)
            {
                Debug.Log("Game.Flags.IsPlayerInputBlocked");
                _hands[Hand.Left].UpdateState(true);
                _hands[Hand.Right].UpdateState(true);   
            }
            else
            {
                _hands[Hand.Left].UpdateState(_isDead);
                _hands[Hand.Right].UpdateState(_isDead);   
            }

            bool aiming = _hands[Hand.Left].IsAiming || _hands[Hand.Right].IsAiming;
            
            //targetFov = aiming ? camFovAim : camFovIdle;
            targetFov =  camFovIdle;

            /*
            _weaponsTargetsParent.position = Vector3.Lerp(_weaponsTargetsParent.position,  Game.Player.MainCamera.transform.position, gunMoveSpeed * Time.deltaTime);
            _weaponsTargetsParent.rotation = Quaternion.Slerp(_weaponsTargetsParent.rotation, Game.Player.MainCamera.transform.rotation, gunRotationSpeed * Time.deltaTime);
            */

            _hands[Hand.Left].MoveHand();
            _hands[Hand.Right].MoveHand();
        }


        private void FixedUpdate()
        {
            if (Game.LocalPlayer == null) return;
            if (_isDead)
                return;
        
            if (Game.Flags.IsPlayerInputBlocked)
                return;
            
            _hands[Hand.Left].UpdateCollision();
            _hands[Hand.Right].UpdateCollision();
        }

        private void LateUpdate()
        {
            if (Game.LocalPlayer == null)
                return;
            if (Shop.Instance && Shop.Instance.IsActive)
                return;
        
            _hands[Hand.Left].UpdateWeaponPosition();
            _hands[Hand.Right].UpdateWeaponPosition();

            Game.LocalPlayer.MainCamera.fieldOfView = Mathf.Lerp(Game.LocalPlayer.MainCamera.fieldOfView, targetFov, fovChangeSpeed * Time.deltaTime);
        }

        public void SetWeapon(WeaponController weapon, Hand hand)
        {
            Game.LocalPlayer.Inventory.SetWeapon(weapon, hand);
            if (weapon)
                weapon.transform.parent = _weaponsParent;
            
            if (_hands[hand].Weapon != null)
            {
                Destroy(_hands[hand].Weapon.gameObject);
            }
            _hands[hand].Weapon = weapon;
        }

        public void ClearHand(Hand hand)
        {
            if (_hands[hand].Weapon != null)
                Destroy(_hands[hand].Weapon.gameObject);
            _hands[hand].Weapon = null;
        }
        
        public PlayerInventory.EquipmentSlot.Slot RemoveWeapon(WeaponController weapon)
        {
            foreach (var hand in _hands)
            {
                if (hand.Value.Weapon == weapon)
                {
                    Destroy(weapon.gameObject);
                    hand.Value.Weapon = null;
                    if (hand.Key == Hand.Left)
                        return PlayerInventory.EquipmentSlot.Slot.LeftHand;
                    
                    return PlayerInventory.EquipmentSlot.Slot.RightHand;
                }
            }
            Debug.LogError("CANT FIND WEAPON TO REMOVE, CHECK THE CODE. THIS ONE SHOULDNT BE CALLED");
            return PlayerInventory.EquipmentSlot.Slot.Body;
        }


        public void CooldownOnAttackInput()
        {
            foreach (var weaponHand in _hands)
            {
                weaponHand.Value.CooldownOnAttack();
            }
        }

        public void Death()
        {
        }
        public void Resurrect()
        {
        }
    }
}