using MrPink.Health;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class PlayerWeaponControls : MonoBehaviour
    {
        public Transform weaponsTargetsParent;
        public Transform weaponsParent;
        
        [Header("LEFT GUN")]
        public WeaponController leftWeapon;
        public Transform deathTransformLeft;
        public Transform idleTransformLeft;
        public Transform aimTransformLeft;
        public Transform reloadTransformLeft;
        
        [Header("RIGHT GUN")]
        public WeaponController rightWeapon;
        public Transform deathTransformRight;
        public Transform idleTransformRight;
        public Transform aimTransformRight;
        public Transform reloadTransformRight;
        
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
    
        Transform targetLeftTransform;
        Transform targetRightTransform;
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
            targetLeftTransform = idleTransformLeft;
            targetRightTransform = idleTransformRight;
            
            if (_isDead)
            {
                aiming = true;
                targetLeftTransform = deathTransformLeft;
                targetRightTransform = deathTransformRight;
            }
            else
            {
                if (leftWeapon)
                {
                    if (leftWeapon.OnCooldown || weaponCollidesWithWallLeft)
                        targetLeftTransform = reloadTransformLeft;
                    
                    else if (LevelGenerator.Instance.levelIsReady)
                    {
                        if (Input.GetMouseButton(0))
                        {
                            aiming = true;
                            targetLeftTransform = aimTransformLeft;
                        }

                        if (Input.GetMouseButtonUp(0))
                            leftWeapon.Shot(Player.Health);
                    }
                }

                if (rightWeapon)
                {
                    if (rightWeapon.OnCooldown || weaponCollidesWithWallRight)
                        targetRightTransform = reloadTransformRight;
                    
                    else if (LevelGenerator.Instance.levelIsReady)
                    {
                        if (Input.GetMouseButton(1))
                        {
                            aiming = true;
                            targetRightTransform = aimTransformRight;
                        }

                        if (Input.GetMouseButtonUp(1))
                            rightWeapon.Shot(Player.Health);
                        
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
        
            if (Input.GetMouseButton(0))
            {
                currentTransformToRaycastL = aimTransformLeft;
            }
            else
            {
                currentTransformToRaycastL = idleTransformLeft;
            }
            if (Input.GetMouseButton(1))
            {
                currentTransformToRaycastR = aimTransformRight;
            }
            else
            {
                currentTransformToRaycastR = idleTransformRight;
            }
        
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
        
            if (leftWeapon)
            {
                leftWeapon.transform.position = Vector3.Lerp(leftWeapon.transform.position, targetLeftTransform.position,
                    gunMoveSpeed * Time.deltaTime);
                leftWeapon.transform.rotation = Quaternion.Slerp(leftWeapon.transform.rotation,
                    targetLeftTransform.rotation, gunRotationSpeed * Time.deltaTime);
            }
            if (rightWeapon)
            {
                rightWeapon.transform.position = Vector3.Lerp(rightWeapon.transform.position, targetRightTransform.position,
                    gunMoveSpeed * Time.deltaTime);
                rightWeapon.transform.rotation = Quaternion.Slerp(rightWeapon.transform.rotation,
                    targetRightTransform.rotation, gunRotationSpeed * Time.deltaTime);
            }
        
            Player.MainCamera.fieldOfView = Mathf.Lerp(Player.MainCamera.fieldOfView, targetFov, fovChangeSpeed * Time.deltaTime);
        }

        public void SetLeftWeapon(WeaponController weapon)
        {
            leftWeapon = weapon;
            weapon.transform.parent = weaponsParent;
        }
    
        public void SetRightWeapon(WeaponController weapon)
        {
            rightWeapon = weapon;
            weapon.transform.parent = weaponsParent;
        }
    
        public void Death()
        {
            _isDead = true;
        }
    }
}