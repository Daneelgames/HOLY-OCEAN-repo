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
        private float cooldownOnAttackInput = 0f;
        private float cooldownOnAttackInputMax = 0.5f;

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
                new Vector3(Game.LocalPlayer.Movement.MoveVector.x, - Game.LocalPlayer.Movement.MoveVector.y - Game.LocalPlayer.Movement.rb.velocity.normalized.y * 0.3f, 0) * _weapon.gunsMoveDistanceScaler,  
                gunMoveSpeed * Time.deltaTime);

            var rot = transform.localRotation;
            float mouseX = Input.GetAxis("Mouse X");
            if (Mathf.Abs(mouseX) > _weapon.WeaponRotationZScalerThreshold)
                gunRotationSpeed *= _weapon.WeaponRotationZScaler;
            
            rot.eulerAngles += new Vector3(0, 0, mouseX * gunRotationSpeed * Time.smoothDeltaTime);
            transform.localRotation = Quaternion.Slerp(rot, Quaternion.identity, Time.smoothDeltaTime);
        }

        public void UpdateState(bool isDead)
        {
            if (_isAttacking)
            {
                IsAiming = false;
                return;
            }
            
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
                IsAiming = false;
                CurrentPosition = WeaponPosition.Reload;
                return;
            }
            
            /*
            if (!LevelGenerator.Instance.IsLevelReady)
            {
                IsAiming = false;
                CurrentPosition = WeaponPosition.Reload;
                return;
            }*/

            if (!canShootIfPhoneInUse && DialogueWindowInterface.Instance.dialogueWindowActive)
            {
                IsAiming = false;
                CurrentPosition = WeaponPosition.Reload;
                return;
            }

            if (Game.LocalPlayer.Interactor.carryingPortableRb != null || cooldownOnAttackInput > 0)
                return;
            
            if (Input.GetMouseButton(_mouseButtonIndex))
            {
                IsAiming = true;
                CurrentPosition = Weapon.IsMelee
                    ? WeaponPosition.MeleeAim
                    : WeaponPosition.Aim;
            }

            if (Input.GetMouseButtonUp(_mouseButtonIndex) && IsAiming)
            {
                HandleAttack().ForgetWithHandler();
            }
        }

        public void CooldownOnAttack()
        {
            cooldownOnAttackInput = cooldownOnAttackInputMax;
            if (_cooldownOnAttackCoroutine != null)
                StopCoroutine(_cooldownOnAttackCoroutine);
            _cooldownOnAttackCoroutine = StartCoroutine(CooldownOnAttackCoroutine());
        }

        private Coroutine _cooldownOnAttackCoroutine;

        IEnumerator CooldownOnAttackCoroutine()
        {
            while (cooldownOnAttackInput > 0)
            {
                cooldownOnAttackInput -= Time.deltaTime;
                yield return null;
            }
        }
            
        private async UniTask HandleAttack()
        {
            _isAttacking = true;
            
            if (Weapon.IsMelee)
                CurrentPosition = WeaponPosition.MeleeAttack;

            //var attackTime = await Weapon.Shot(Game.LocalPlayer.Health);
            var attackTime = Weapon.cooldown;
            
            Weapon.Shot(Game.LocalPlayer.Health);

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