using System;
using System.Collections;
using Brezg.Extensions.UniTaskExtensions;
using Brezg.Serialization;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using MrPink.WeaponsSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class WeaponHand : MonoBehaviour
    {
        [SerializeField, SceneObjectsOnly]
        [CanBeNull]
        private WeaponController _weapon;
        
        [SerializeField]
        private Color _gizmoColor = Color.red;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private UnityDictionary<WeaponPosition, Transform> _positions = new UnityDictionary<WeaponPosition, Transform>();

        [SerializeField, Range(0, 2)] 
        private int _mouseButtonIndex;

        [ShowInInspector, ReadOnly]
        private bool _isCollidingWithWall;

#if UNITY_EDITOR
        
        [SerializeField] 
        [BoxGroup("Position Debug")]
        private bool _isDebugPositioningEnabled;
        
        [SerializeField] 
        [BoxGroup("Position Debug")]
        private WeaponPosition _debugPosition;
        
#endif
        
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


        private bool _isAttacking = false;
        
        
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
            IsAiming = false;
            
            if (_isAttacking)
                return;
            
            CurrentPosition = WeaponPosition.Idle;
            
            if (isDead)
            {
                IsAiming = false;
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

            if (Input.GetMouseButton(_mouseButtonIndex) && Player.Interactor.carryingPortableRb == null)
            {
                IsAiming = true;
                CurrentPosition = Weapon.IsMelee
                    ? WeaponPosition.MeleeAim
                    : WeaponPosition.Aim;
            }

            if (Input.GetMouseButtonUp(_mouseButtonIndex))
            {
                HandleAttack().ForgetWithHandler();
            }
        }


        private async UniTask HandleAttack()
        {
            _isAttacking = true;
            
            if (Weapon.IsMelee)
                CurrentPosition = WeaponPosition.MeleeAttack;

            var attackTime = await Weapon.Shot(Player.Health);

            if (Weapon.IsMelee)
                await UniTask.Delay((int) (attackTime * 1000));
            
            _isAttacking = false;
        }

        public void UpdateWeaponPosition()
        {
            if (!IsWeaponEquipped)
                return;
            
#if UNITY_EDITOR

            if (_isDebugPositioningEnabled)
                CurrentPosition = _debugPosition;

#endif

            int meleeAttackScaler = 
                CurrentPosition == WeaponPosition.MeleeAttack 
                    ? 2 
                    : 1;
            
            float gunMoveSpeed = _weapon.gunMoveSpeed * meleeAttackScaler;
            float gunRotationSpeed = _weapon.gunRotationSpeed * meleeAttackScaler;
            float scaler = IsAiming ? _weapon.gunMoveSpeedScaler : 1;
            
            Weapon.transform.position = Vector3.Lerp(Weapon.transform.position,  CurrentTransform.position, gunMoveSpeed * scaler * Time.deltaTime);
            Weapon.transform.rotation = Quaternion.Slerp(Weapon.transform.rotation, CurrentTransform.rotation, gunRotationSpeed * scaler * Time.deltaTime);
        }

        public void UpdateCollision()
        {
            Transform raycastTransform = this[
                CurrentPosition == WeaponPosition.Aim || CurrentPosition == WeaponPosition.MeleeAim
                    ? CurrentPosition
                    : WeaponPosition.Idle
            ];
            
            if (Physics.Raycast(raycastTransform.position,
                    raycastTransform.forward, out var hit,
                    Vector3.Distance(raycastTransform.position, raycastTransform.position + raycastTransform.forward * 0.5f), allSolidsLayerMask))
            {
                _isCollidingWithWall = true;
            }
            else
                _isCollidingWithWall = false;
        }


        private void OnDrawGizmos()
        {
            var from = _positions[WeaponPosition.MeleeAim].position;
            var to = _positions[WeaponPosition.MeleeAttack].position;
            
            Gizmos.color = _gizmoColor;

            Gizmos.DrawSphere(from, 0.1f);
            Gizmos.DrawSphere(to, 0.08f);
            Gizmos.DrawLine(from, to);
        }
    }
}