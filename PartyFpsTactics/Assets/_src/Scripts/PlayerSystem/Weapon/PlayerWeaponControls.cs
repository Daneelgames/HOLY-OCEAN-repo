using Brezg.Serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class PlayerWeaponControls : MonoBehaviour
    {
        public Transform weaponsTargetsParent;
        public Transform weaponsParent;

        [SerializeField, ChildGameObjectsOnly, Required]
        private UnityDictionary<Hand, WeaponHand> _hands = new UnityDictionary<Hand, WeaponHand>();
        
        
        [Header("CAMERA")]
        public float camFovIdle = 90;
        public float camFovAim = 90;
        public float fovChangeSpeed = 90;
        public float gunMoveSpeed = 100;
        public float gunRotationSpeed = 100;
    
        private Transform currentTransformToRaycastL;
        private Transform currentTransformToRaycastR;

        private bool weaponCollidesWithWallLeft = false;
        private bool weaponCollidesWithWallRight = false;
        
        float targetFov = 90;
        
        private bool _isDead = false;


        private void Start()
        {
            weaponsTargetsParent.parent = null;
            /*
        leftWeapon.transform.parent = null;
        rightWeapon.transform.parent = null;
        */
        }

        private void Update()
        {
            if (Shop.Instance && Shop.Instance.IsActive)
                return;
        
            if (gunMoveSpeed < 1) 
                gunMoveSpeed = 1;
        
            bool aiming = false;

            _hands[Hand.Left].CurrentPosition = WeaponPosition.Idle;
            _hands[Hand.Right].CurrentPosition = WeaponPosition.Idle;

            if (_isDead)
            {
                aiming = true;
                _hands[Hand.Left].CurrentPosition = WeaponPosition.Death;
                _hands[Hand.Right].CurrentPosition = WeaponPosition.Death;
            }
            else
            {
                if (_hands[Hand.Left].IsWeaponEquipped)
                {
                    if (_hands[Hand.Left].Weapon.OnCooldown || weaponCollidesWithWallLeft)
                        _hands[Hand.Left].CurrentPosition = WeaponPosition.Reload;
                    
                    else if (LevelGenerator.Instance.levelIsReady)
                    {
                        if (Input.GetMouseButton(0))
                        {
                            aiming = true;
                            _hands[Hand.Left].CurrentPosition = WeaponPosition.Aim;
                        }

                        if (Input.GetMouseButtonUp(0))
                            _hands[Hand.Left].Weapon.Shot(Player.Health);
                    }
                }

                if (_hands[Hand.Right].IsWeaponEquipped)
                {
                    if (_hands[Hand.Right].Weapon.OnCooldown || weaponCollidesWithWallRight)
                        _hands[Hand.Right].CurrentPosition = WeaponPosition.Reload;
                    
                    else if (LevelGenerator.Instance.levelIsReady)
                    {
                        if (Input.GetMouseButton(1))
                        {
                            aiming = true;
                            _hands[Hand.Right].CurrentPosition = WeaponPosition.Aim;
                        }

                        if (Input.GetMouseButtonUp(1))
                            _hands[Hand.Right].Weapon.Shot(Player.Health);
                        
                    }
                }
            }
            targetFov = aiming ? camFovAim : camFovIdle;

            weaponsTargetsParent.position = Vector3.Lerp(weaponsTargetsParent.position,  Player.MainCamera.transform.position, gunMoveSpeed * Time.deltaTime);
            weaponsTargetsParent.rotation = Quaternion.Slerp(weaponsTargetsParent.rotation, Player.MainCamera.transform.rotation, gunRotationSpeed * Time.deltaTime);
        }


        private void FixedUpdate()
        {
            if (_isDead)
                return;
        
            if (!LevelGenerator.Instance.levelIsReady)
                return;
        
            
            if (_hands[Hand.Left].CurrentPosition == WeaponPosition.Aim)
                currentTransformToRaycastL = _hands[Hand.Left][WeaponPosition.Aim];
            else
                currentTransformToRaycastL = _hands[Hand.Left][WeaponPosition.Idle];
            
            if (_hands[Hand.Right].CurrentPosition == WeaponPosition.Aim)
                currentTransformToRaycastL = _hands[Hand.Right][WeaponPosition.Aim];
            else
                currentTransformToRaycastR = _hands[Hand.Right][WeaponPosition.Idle];
            
        
            if (Physics.Raycast(currentTransformToRaycastL.position,
                    currentTransformToRaycastL.forward, out var hit,
                    Vector3.Distance(currentTransformToRaycastL.position, currentTransformToRaycastL.position + currentTransformToRaycastL.forward * 0.5f), 1 << 6))
            {
                weaponCollidesWithWallLeft = true;
            }
            else
            {
                weaponCollidesWithWallLeft = false;
            }
            
            if (Physics.Raycast(currentTransformToRaycastR.position,
                    currentTransformToRaycastR.forward, out var hitR,
                    Vector3.Distance(currentTransformToRaycastR.position, currentTransformToRaycastR.position + currentTransformToRaycastR.forward * 0.5f), 1 << 6))
            {
                weaponCollidesWithWallRight = true;
            }
            else
            {
                weaponCollidesWithWallRight = false;
            }
        }

        private void LateUpdate()
        {
            if (Shop.Instance && Shop.Instance.IsActive)
                return;
        
            if (_hands[Hand.Left].IsWeaponEquipped)
            {
                _hands[Hand.Left].Weapon.transform.position = Vector3.Lerp(_hands[Hand.Left].Weapon.transform.position,  _hands[Hand.Left].CurrentTransform.position,
                    gunMoveSpeed * Time.deltaTime);
                _hands[Hand.Left].Weapon.transform.rotation = Quaternion.Slerp(_hands[Hand.Left].Weapon.transform.rotation,
                    _hands[Hand.Left].CurrentTransform.rotation, gunRotationSpeed * Time.deltaTime);
            }
            if (_hands[Hand.Right].IsWeaponEquipped)
            {
                _hands[Hand.Right].Weapon.transform.position = Vector3.Lerp(_hands[Hand.Right].Weapon.transform.position, _hands[Hand.Right].CurrentTransform.position,
                    gunMoveSpeed * Time.deltaTime);
                _hands[Hand.Right].Weapon.transform.rotation = Quaternion.Slerp(_hands[Hand.Right].Weapon.transform.rotation,
                    _hands[Hand.Right].CurrentTransform.rotation, gunRotationSpeed * Time.deltaTime);
            }
        
            Player.MainCamera.fieldOfView = Mathf.Lerp(Player.MainCamera.fieldOfView, targetFov, fovChangeSpeed * Time.deltaTime);
        }

        public void SetLeftWeapon(WeaponController weapon)
        {
            _hands[Hand.Left].Weapon = weapon;
            weapon.transform.parent = weaponsParent;
        }
    
        public void SetRightWeapon(WeaponController weapon)
        {
            _hands[Hand.Right].Weapon = weapon;
            weapon.transform.parent = weaponsParent;
        }
    
        public void Death()
        {
            _isDead = true;
        }
    }
}