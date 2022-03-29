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

        
        public WeaponController Weapon
        {
            get => _weapon;
            set => _weapon = value;
        }


        public void MoveHand(float gunMoveSpeed)
        {
            if (!_weapon)
                return;
            
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(Player.Movement.MoveVector.x, Player.Movement.MoveVector.y + Player.Movement.rb.velocity.y * 0.3f, 0) 
                                                                            * _weapon.gunsMoveDistanceScaler,  gunMoveSpeed * Time.deltaTime);
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

            if (Input.GetMouseButton(_mouseButtonIndex))
            {
                IsAiming = true;
                CurrentPosition = WeaponPosition.Aim;
            }
            
            if (Input.GetMouseButtonUp(_mouseButtonIndex))
                Weapon.Shot(Player.Health);
        }

        public void UpdateWeaponPosition(float gunMoveSpeed, float gunRotationSpeed)
        {
            if (!IsWeaponEquipped)
                return;
            
            Weapon.transform.position = Vector3.Lerp(Weapon.transform.position,  CurrentTransform.position, gunMoveSpeed * Time.deltaTime);
            Weapon.transform.rotation = Quaternion.Slerp(Weapon.transform.rotation, CurrentTransform.rotation, gunRotationSpeed * Time.deltaTime);
        }

        public void UpdateCollision()
        {
            Transform raycastTransform = 
                CurrentPosition == WeaponPosition.Aim ? 
                    this[WeaponPosition.Aim] : 
                    this[WeaponPosition.Idle];
            
            if (Physics.Raycast(raycastTransform.position,
                    raycastTransform.forward, out var hit,
                    Vector3.Distance(raycastTransform.position, raycastTransform.position + raycastTransform.forward * 0.5f), 1 << 6))
            {
                _isCollidingWithWall = true;
            }
            else
                _isCollidingWithWall = false;
        }
    }
}