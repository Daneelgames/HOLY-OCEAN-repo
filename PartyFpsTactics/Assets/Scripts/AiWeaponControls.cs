using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AiWeaponControls : MonoBehaviour
{
    public WeaponController activeWeapon;
    private HealthController hc;
    public float minAngleToRotateGun = 30;
    public float minAngleToShoot = 15;
    
    private void Awake()
    {
        hc = GetComponent<HealthController>();
    }

    private void Start()
    {
        StartCoroutine(CheckIfNeedFireWeapon());
    }

    IEnumerator CheckIfNeedFireWeapon()
    {
        while (hc.health > 0)
        {
            yield return null;
            
            if (activeWeapon.OnCooldown)
            {
                continue;
            }
            
            if (hc.AiMovement.enemyToLookAt != null)
            {
                Vector3 targetDir = hc.AiMovement.enemyToLookAt.visibilityTrigger.transform.position - transform.position;
                float angle = Vector3.Angle(targetDir, transform.forward);
                if (angle < minAngleToRotateGun)
                {
                    activeWeapon.transform.LookAt(hc.AiMovement.enemyToLookAt.visibilityTrigger.transform.position);
                }
                else
                {
                    activeWeapon.transform.localRotation = activeWeapon.InitLocalRotation;
                }
                if (angle < minAngleToShoot)
                {
                    activeWeapon.Shot(hc);
                }
            }
        }
    }
}
