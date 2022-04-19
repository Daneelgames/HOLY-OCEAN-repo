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
        public float minAngleToRotateGun = 30;
        public float minAngleToShoot = 15;
        public bool rotateWeaponTowardTarget = true;
        public float maxDistanceToAttack = 1000;
        public float updateRate = 0.1f;
        public float maxDistanceFromPlayerToShoot = 250;
        
        private Unit _selfUnit;

        private void Awake()
        {
            _selfUnit = GetComponent<Unit>();
        }

        private void Start()
        {
            StartCoroutine(CheckIfNeedFireWeapon());
        }

        private IEnumerator CheckIfNeedFireWeapon()
        {
            while (_selfUnit.HealthController.health > 0)
            {
                if (updateRate > 0)
                    yield return new WaitForSeconds(updateRate);
                else
                    yield return null;
                
                if (Vector3.Distance(transform.position, Game.Player.MainCamera.transform.position) > maxDistanceFromPlayerToShoot)
                    continue;

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

            if (_selfUnit.HealthController.AiMovement.enemyToLookAt == null)
                yield break;
            
            if (Vector3.Distance(transform.position, _selfUnit.HealthController.AiMovement.enemyToLookAt.visibilityTrigger.transform.position) > maxDistanceToAttack)
                yield break;

            if (_selfUnit.HealthController.AiMovement.enemyToLookAt.gameObject == Game.Player.GameObject)
            {
                var isNotInPlayerPov = !GameManager.Instance.IsPositionInPlayerFov(activeWeapon.transform.position);
                var isCoinFlippedRight = Random.value > 0.5f;
                    
                if (isNotInPlayerPov && isCoinFlippedRight)
                    yield break;
            }

            Vector3 targetDir = _selfUnit.HealthController.AiMovement.enemyToLookAt.visibilityTrigger.transform.position - transform.position;
            float angle = Vector3.Angle(targetDir, transform.forward);
            Vector3 offset = Vector3.zero;
            if (_selfUnit.HealthController.AiMovement.enemyToLookAt.playerMovement)
                offset = _selfUnit.HealthController.AiMovement.enemyToLookAt.playerMovement.rb.velocity;

            if (rotateWeaponTowardTarget)
            {
                if (angle < minAngleToRotateGun)
                    activeWeapon.transform.LookAt(_selfUnit.HealthController.AiMovement.enemyToLookAt.visibilityTrigger.transform.position +
                                                  offset);
                else
                    activeWeapon.transform.localRotation = activeWeapon.InitLocalRotation;
            }

            targetDir = _selfUnit.HealthController.AiMovement.enemyToLookAt.visibilityTrigger.transform.position - transform.position;
            angle = Vector3.Angle(targetDir, transform.forward);
                
            if (angle < minAngleToShoot)
            {
                activeWeapon.Shot(_selfUnit.HealthController, _selfUnit.HealthController.AiMovement.enemyToLookAt.visibilityTrigger.transform).ForgetWithHandler();
                yield return new WaitForSeconds(Random.Range(weaponsAttackSwitchCooldownMinMax.x, weaponsAttackSwitchCooldownMinMax.y));
            }
        }
    }
}