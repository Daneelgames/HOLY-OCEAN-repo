using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class PlayerWeaponControls : MonoBehaviour
{
    [Header("LEFT")]
    public WeaponController leftWeapon;
    public Transform idleTransformLeft;
    public Transform aimTransformLeft;
    public Transform reloadTransformLeft;
    [Header("RIGHT")]
    public WeaponController rightWeapon;
    public Transform idleTransformRight;
    public Transform aimTransformRight;
    public Transform reloadTransformRight;
    [Header("CAMERA")]
    public float camFovIdle = 90;
    public float camFovAim = 90;
    public float aimFovChangeSpeed = 1;
    public float idleFovChangeSpeed = 90;
    public float gunMoveSpeed = 100;
    public float gunRotationSpeed = 100;
    private Transform currentTransformToRaycastL;
    private Transform currentTransformToRaycastR;
    private HealthController hc;

    private bool weaponCollidesWithWallLeft = false;
    private bool weaponCollidesWithWallRight = false;
    
    
    Transform targetLeftTransform;
    Transform targetRightTransform;
    float targetFov = 90;
    void Start()
    {
        hc = gameObject.GetComponent<HealthController>();
        leftWeapon.transform.parent = null;
        rightWeapon.transform.parent = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            gunMoveSpeed--;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            gunMoveSpeed++;
        }
        

        if (gunMoveSpeed < 1) gunMoveSpeed = 1;
        
        bool aiming = false;
        targetLeftTransform = idleTransformLeft;
        targetRightTransform = idleTransformRight;
        if (leftWeapon.OnCooldown || weaponCollidesWithWallLeft)
        {
            targetLeftTransform = reloadTransformLeft;
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                aiming = true;
                targetLeftTransform = aimTransformLeft;
            }
            if (Input.GetMouseButtonUp(0))
            {
                leftWeapon.Shot(hc);
            }
            if (Input.GetKeyDown(KeyCode.F))
            {
                leftWeapon.Shot(hc);
            }
        }
        
        if (rightWeapon.OnCooldown || weaponCollidesWithWallRight)
        {
            targetRightTransform = reloadTransformRight;
        }
        else
        {
            if (Input.GetMouseButton(1))
            {
                aiming = true;
                targetRightTransform = aimTransformRight;
            }
        
            if (Input.GetMouseButtonUp(1))
            {
                rightWeapon.Shot(hc);
            }
        }

        targetFov = aiming ? camFovAim : camFovIdle;
        
    }

    private void FixedUpdate()
    {
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
        leftWeapon.transform.position = Vector3.Lerp(leftWeapon.transform.position, targetLeftTransform.position, gunMoveSpeed * Time.deltaTime);
        leftWeapon.transform.rotation = Quaternion.Slerp(leftWeapon.transform.rotation, targetLeftTransform.rotation, gunRotationSpeed * Time.deltaTime);
        rightWeapon.transform.position = Vector3.Lerp(rightWeapon.transform.position, targetRightTransform.position, gunMoveSpeed * Time.deltaTime);
        rightWeapon.transform.rotation = Quaternion.Slerp(rightWeapon.transform.rotation, targetRightTransform.rotation, gunRotationSpeed * Time.deltaTime);
        
        PlayerMovement.Instance.MainCam.fieldOfView = Mathf.Lerp(PlayerMovement.Instance.MainCam.fieldOfView, targetFov, idleFovChangeSpeed * Time.deltaTime);
    }
}
