using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponControls : MonoBehaviour
{
    public WeaponController activeWeapon;
    public float camFovIdle = 90;
    public float camFovAim = 90;
    public float aimFovChangeSpeed = 1;
    public float idleFovChangeSpeed = 90;
    public float gunMoveSpeed = 100;
    public float gunRotationSpeed = 100;
    public Transform idleTransform;
    public Transform aimTransform;
    public Transform reloadTransform;
    private Transform currentTransformToRaycast;
    private HealthController hc;

    private bool weaponCollidesWithWall = false;
    void Start()
    {
        hc = gameObject.GetComponent<HealthController>();
        activeWeapon.transform.parent = null;
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            currentTransformToRaycast = aimTransform;
        }
        else
        {
            currentTransformToRaycast = idleTransform;
        }
        
        if (Physics.Raycast(currentTransformToRaycast.position,
            currentTransformToRaycast.forward, out var hit,
            Vector3.Distance(currentTransformToRaycast.position, currentTransformToRaycast.position + currentTransformToRaycast.forward * 0.5f), 1 << 6))
        {
            weaponCollidesWithWall = true;
        }
        else
        {
            weaponCollidesWithWall = false;
        }
    }

    void Update()
    {
        if (activeWeapon.OnCooldown || weaponCollidesWithWall)
        {
            activeWeapon.transform.position = Vector3.Lerp(activeWeapon.transform.position, reloadTransform.position,gunMoveSpeed * Time.smoothDeltaTime);
            activeWeapon.transform.rotation = Quaternion.Slerp(activeWeapon.transform.rotation, reloadTransform.rotation, gunRotationSpeed * Time.smoothDeltaTime);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            activeWeapon.Shot(hc);
        }
        
        if (Input.GetMouseButton(1))
        {
            PlayerMovement.Instance.MainCam.fieldOfView = Mathf.Lerp(PlayerMovement.Instance.MainCam.fieldOfView, camFovAim, aimFovChangeSpeed * Time.smoothDeltaTime);
            activeWeapon.transform.position = Vector3.Lerp(activeWeapon.transform.position, aimTransform.position, gunMoveSpeed * Time.smoothDeltaTime);
            activeWeapon.transform.rotation = Quaternion.Slerp(activeWeapon.transform.rotation, aimTransform.rotation, gunRotationSpeed * Time.smoothDeltaTime);
            return;
        }
        
        
        PlayerMovement.Instance.MainCam.fieldOfView = Mathf.Lerp(PlayerMovement.Instance.MainCam.fieldOfView, camFovIdle, idleFovChangeSpeed * Time.smoothDeltaTime);
        activeWeapon.transform.position = Vector3.Lerp(activeWeapon.transform.position, idleTransform.position, gunMoveSpeed * Time.smoothDeltaTime);
        activeWeapon.transform.rotation = Quaternion.Slerp(activeWeapon.transform.rotation, idleTransform.rotation, gunRotationSpeed * Time.smoothDeltaTime);
    }
}
