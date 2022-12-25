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
            if (Shop.Instance && Shop.Instance.IsActive)
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
            
            targetFov = aiming ? camFovAim : camFovIdle;

            /*
            _weaponsTargetsParent.position = Vector3.Lerp(_weaponsTargetsParent.position,  Game.Player.MainCamera.transform.position, gunMoveSpeed * Time.deltaTime);
            _weaponsTargetsParent.rotation = Quaternion.Slerp(_weaponsTargetsParent.rotation, Game.Player.MainCamera.transform.rotation, gunRotationSpeed * Time.deltaTime);
            */

            _hands[Hand.Left].MoveHand();
            _hands[Hand.Right].MoveHand();
        }


        private void FixedUpdate()
        {
            if (_isDead)
                return;
        
            if (Game.Flags.IsPlayerInputBlocked)
                return;
            
            _hands[Hand.Left].UpdateCollision();
            _hands[Hand.Right].UpdateCollision();
        }

        private void LateUpdate()
        {
            if (Shop.Instance && Shop.Instance.IsActive)
                return;
        
            _hands[Hand.Left].UpdateWeaponPosition();
            _hands[Hand.Right].UpdateWeaponPosition();

            Game.LocalPlayer.MainCamera.fieldOfView = Mathf.Lerp(Game.LocalPlayer.MainCamera.fieldOfView, targetFov, fovChangeSpeed * Time.deltaTime);
        }

        public void SetWeapon(WeaponController weapon, Hand hand)
        {
            PlayerInventory.Instance.SetWeapon(weapon, hand);
            if (weapon)
                weapon.transform.parent = _weaponsParent;
            
            if (_hands[hand].Weapon != null)
            {
                Destroy(_hands[hand].Weapon.gameObject);
            }
            _hands[hand].Weapon = weapon;
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