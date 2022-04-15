using System.Collections;
using System.Collections.Generic;
using Brezg.Extensions.UniTaskExtensions;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.WeaponsSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MrPink.Units
{
    public class UnitAiWeaponControls : MonoBehaviour
    {
        public List<WeaponController> activeWeapons;
        public Vector2 weaponsAttackSwitchCooldownMinMax = new Vector2(0, 0);
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

        private IEnumerator CheckIfNeedFireWeapon()
        {
            while (hc.health > 0)
            {
                yield return null;

                foreach (var activeWeapon in activeWeapons)
                    yield return MakeWeaponDecision(activeWeapon);
            }
        }

        private IEnumerator MakeWeaponDecision(WeaponController activeWeapon)
        {
            if (!activeWeapon)
                yield break;
                
            if (activeWeapon.OnCooldown)
                yield break;

            if (hc.AiMovement.enemyToLookAt == null)
                yield break;


            if (hc.AiMovement.enemyToLookAt.gameObject == Player.GameObject)
            {
                var isNotInPlayerPov = !GameManager.Instance.IsPositionInPlayerFov(activeWeapon.transform.position);
                var isCoinFlippedRight = Random.value > 0.5f;
                    
                if (isNotInPlayerPov && isCoinFlippedRight)
                    yield break;
            }

            Vector3 targetDir = hc.AiMovement.enemyToLookAt.visibilityTrigger.transform.position - transform.position;
            float angle = Vector3.Angle(targetDir, transform.forward);
            Vector3 offset = Vector3.zero;
            if (hc.AiMovement.enemyToLookAt.playerMovement)
                offset = hc.AiMovement.enemyToLookAt.playerMovement.rb.velocity;

            if (angle < minAngleToRotateGun)
                activeWeapon.transform.LookAt(hc.AiMovement.enemyToLookAt.visibilityTrigger.transform.position + offset);
            else
                activeWeapon.transform.localRotation = activeWeapon.InitLocalRotation;

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