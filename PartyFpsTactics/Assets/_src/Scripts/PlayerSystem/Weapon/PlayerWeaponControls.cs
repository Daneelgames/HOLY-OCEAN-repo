using Brezg.Serialization;
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
        
        
        [Header("CAMERA")]
        public float camFovIdle = 90;
        public float camFovAim = 90;
        public float fovChangeSpeed = 90;
        public float gunMoveSpeed = 100;
        public float gunRotationSpeed = 100;
        

        float targetFov = 90;
        
        private bool _isDead = false;
        

        private void Update()
        {
            if (Shop.Instance && Shop.Instance.IsActive)
                return;
        
            if (gunMoveSpeed < 1) 
                gunMoveSpeed = 1;
            
            _hands[Hand.Left].UpdateState(_isDead);
            _hands[Hand.Right].UpdateState(_isDead);

            bool aiming = _hands[Hand.Left].IsAiming || _hands[Hand.Right].IsAiming;
            
            targetFov = aiming ? camFovAim : camFovIdle;

            _weaponsTargetsParent.position = Vector3.Lerp(_weaponsTargetsParent.position,  Player.MainCamera.transform.position, gunMoveSpeed * Time.deltaTime);
            _weaponsTargetsParent.rotation = Quaternion.Slerp(_weaponsTargetsParent.rotation, Player.MainCamera.transform.rotation, gunRotationSpeed * Time.deltaTime);

            _hands[Hand.Left].MoveHand(gunMoveSpeed);
            _hands[Hand.Right].MoveHand(gunMoveSpeed);
        }


        private void FixedUpdate()
        {
            if (_isDead)
                return;
        
            if (!LevelGenerator.Instance.levelIsReady)
                return;
            
            _hands[Hand.Left].UpdateCollision();
            _hands[Hand.Right].UpdateCollision();
        }

        private void LateUpdate()
        {
            if (Shop.Instance && Shop.Instance.IsActive)
                return;
        
            _hands[Hand.Left].UpdateWeaponPosition(gunMoveSpeed, gunRotationSpeed);
            _hands[Hand.Right].UpdateWeaponPosition(gunMoveSpeed, gunRotationSpeed);

            Player.MainCamera.fieldOfView = Mathf.Lerp(Player.MainCamera.fieldOfView, targetFov, fovChangeSpeed * Time.deltaTime);
        }

        public void SetWeapon(WeaponController weapon, Hand hand)
        {
            _hands[hand].Weapon = weapon;
            weapon.transform.parent = _weaponsParent;
        }

        public void Death()
        {
            _isDead = true;
        }
    }
}