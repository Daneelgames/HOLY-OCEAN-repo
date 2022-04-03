using System;
using Brezg.Serialization;
using JetBrains.Annotations;
using MrPink.WeaponsSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MrPink.PlayerSystem
{
    public class WeaponHand : MonoBehaviour
    {
        [SerializeField, SceneObjectsOnly]
        [CanBeNull]
        private WeaponController _weapon;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private UnityDictionary<WeaponPosition, Transform> _positions = new UnityDictionary<WeaponPosition, Transform>();

        [SerializeField, Range(0, 2)] 
        private int _mouseButtonIndex;

        [ShowInInspector, ReadOnly]
        private bool _isCollidingWithWall;
        
        [ShowInInspector, ReadOnly]
        public WeaponPosition CurrentPosition { get; set; } = WeaponPosition.Idle;

        [ShowInInspector, ReadOnly]
        public bool IsAiming { get; private set; }
        
        public Transform CurrentTransform
            => this[CurrentPosition];

        public bool IsWeaponEquipped
            => _weapon != null;

        public Transform this[WeaponPosition position]
            => _positions[position];

        public LayerMask allSolidsLayerMask;
        public bool canShootIfPhoneInUse = true;
        
        public WeaponController Weapon
        {
            get => _weapon;
            set => _weapon = value;
        }

        public void MoveHand()
        {
            if (!_weapon)
                return;
            float gunMoveSpeed = _weapon.gunMoveSpeed;
            float gunRotationSpeed = _weapon.gunRotationSpeed;
            
            transform.localPosition = Vector3.Lerp(transform.localPosition, 
                new Vector3(Player.Movement.MoveVector.x, - Player.Movement.MoveVector.y - Player.Movement.rb.velocity.normalized.y * 0.3f, 0) * _weapon.gunsMoveDistanceScaler,  
                gunMoveSpeed * Time.deltaTime);

            var rot = transform.localRotation;
            float mouseX = Input.GetAxis("Mouse X");
            if (Mathf.Abs(mouseX) > _weapon.WeaponRotationZScalerThreshold)
                gunRotationSpeed *= _weapon.WeaponRotationZScaler;
            
            rot.eulerAngles += new Vector3(0, 0, mouseX * gunRotationSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(rot, Quaternion.identity, Time.deltaTime);
        }

        public void UpdateState(bool isDead)
        {
            CurrentPosition = WeaponPosition.Idle;
            IsAiming = false;
            
            if (isDead)
            {
                IsAiming = true;
                CurrentPosition = WeaponPosition.Death;
                return;
            }
            
            if (!IsWeaponEquipped)
                return;

            if (Weapon.OnCooldown || _isCollidingWithWall)
            {
                CurrentPosition = WeaponPosition.Reload;
                return;
            }
            
            if (!LevelGenerator.Instance.levelIsReady)
                return;

            if (!canShootIfPhoneInUse && DialogueWindowInterface.Instance.dialogueWindowActive)
            {
                CurrentPosition = WeaponPosition.Reload;
                return;
            }
            
            if (Input.GetMouseButton(_mouseButtonIndex))
            {
                IsAiming = true;
                CurrentPosition = WeaponPosition.Aim;
            }

            if (Input.GetMouseButtonUp(_mouseButtonIndex))
                Weapon.Shot(Player.Health);
        }

        public void UpdateWeaponPosition()
        {
            if (!IsWeaponEquipped)
                return;

            float gunMoveSpeed = _weapon.gunMoveSpeed;
            float gunRotationSpeed = _weapon.gunRotationSpeed;
            
            float scaler = 1;
            float scalerRot = 1;
            if (IsAiming)
            {
                scaler = _weapon.gunMoveSpeedScaler;
                scalerRot = _weapon.gunRotSpeedScaler;
            }
            Weapon.transform.position = Vector3.Lerp(Weapon.transform.position,  CurrentTransform.position, gunMoveSpeed * scaler * Time.deltaTime);
            Weapon.transform.rotation = Quaternion.Slerp(Weapon.transform.rotation, CurrentTransform.rotation, gunRotationSpeed * scaler * Time.deltaTime);
        }

        public void UpdateCollision()
        {
            Transform raycastTransform = 
                CurrentPosition == WeaponPosition.Aim ? 
                    this[WeaponPosition.Aim] : 
                    this[WeaponPosition.Idle];
            
            if (Physics.Raycast(raycastTransform.position,
                    raycastTransform.forward, out var hit,
                    Vector3.Distance(raycastTransform.position, raycastTransform.position + raycastTransform.forward * 0.5f), allSolidsLayerMask))
            {
                _isCollidingWithWall = true;
            }
            else
                _isCollidingWithWall = false;
        }
    }
}