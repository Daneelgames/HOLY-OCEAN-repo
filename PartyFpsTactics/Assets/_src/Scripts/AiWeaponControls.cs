using System.Collections;
using System.Collections.Generic;
using Brezg.Extensions.UniTaskExtensions;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.WeaponsSystem;
using UnityEngine;
using Random = UnityEngine.Random;

public class AiWeaponControls : MonoBehaviour
{
    public List<WeaponController> activeWeapons;
    public Vector2 weaponsAttackSwitchCooldownMinMax = new Vector2(0,0);
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

            for (int i = 0; i < activeWeapons.Count; i++)
            {
                var activeWeapon = activeWeapons[i];
                
                if (!activeWeapon || activeWeapon.OnCooldown)
                {
                    continue;
                }
            
                if (hc.AiMovement.enemyToLookAt != null)
                {
                    if (hc.AiMovement.enemyToLookAt.gameObject == Player.GameObject)
                    {
                        if (!GameManager.Instance.IsPositionInPlayerFov(activeWeapon.transform.position) && Random.value > 0.5f)
                        {
                            continue;   
                        }
                    }
                
                    Vector3 targetDir = hc.AiMovement.enemyToLookAt.visibilityTrigger.transform.position - transform.position;
                    float angle = Vector3.Angle(targetDir, transform.forward);
                    Vector3 offset = Vector3.zero;
                    if (hc.AiMovement.enemyToLookAt.playerMovement)
                        offset = hc.AiMovement.enemyToLookAt.playerMovement.rb.velocity;
                
                    if (angle < minAngleToRotateGun)
                    {
                        activeWeapon.transform.LookAt(hc.AiMovement.enemyToLookAt.visibilityTrigger.transform.position + offset);
                    }
                    else
                    {
                        activeWeapon.transform.localRotation = activeWeapon.InitLocalRotation;
                    }
                    targetDir = hc.AiMovement.enemyToLookAt.visibilityTrigger.transform.position - transform.position;
                    angle = Vector3.Angle(targetDir, transform.forward);
                    if (angle < minAngleToShoot)
                    {
                        activeWeapon.Shot(hc, hc.AiMovement.enemyToLookAt.visibilityTrigger.transform).ForgetWithHandler();
                        yield return new WaitForSeconds(Random.Range(weaponsAttackSwitchCooldownMinMax.x, weaponsAttackSwitchCooldownMinMax.y));
                    }
                }
            }
        }
    }
}
