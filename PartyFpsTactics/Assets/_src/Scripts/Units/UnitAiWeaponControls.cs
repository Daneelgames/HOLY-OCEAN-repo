using System.Collections;
using System.Collections.Generic;
using Brezg.Extensions.UniTaskExtensions;
using FishNet.Object;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.WeaponsSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MrPink.Units
{
    public class UnitAiWeaponControls : NetworkBehaviour
    {
        public List<WeaponController> activeWeapons;
        public List<Transform> handsIk;
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

        private void OnEnable()
        {
            StartCoroutine(CheckIfNeedFireWeapon());
        }

        private IEnumerator CheckIfNeedFireWeapon()
        {
            while (_selfUnit.HealthController.health > 0)
            {
                if (IsServer == false)
                {
                    yield return new WaitForSeconds(1);
                    continue;
                }
                if (updateRate > 0)
                    yield return new WaitForSeconds(updateRate);
                else
                    yield return null;
                
                if (Vector3.Distance(transform.position, Game.LocalPlayer.MainCamera.transform.position) > maxDistanceFromPlayerToShoot)
                    continue;

                for (var index = 0; index < activeWeapons.Count; index++)
                {
                    var activeWeapon = activeWeapons[index];
                    Transform hand = null;
                    if (handsIk.Count > index)
                        hand = handsIk[index];
                    yield return MakeWeaponDecision(activeWeapon, hand);
                }
            }
        }

        private IEnumerator MakeWeaponDecision(WeaponController activeWeapon, Transform handIk)
        {
            if (!activeWeapon)
                yield break;
                
            if (activeWeapon.OnCooldown)
                yield break;

            var enemyToShoot = _selfUnit.HealthController.AiMovement.enemyToLookAt;
            if (enemyToShoot == null || enemyToShoot.health <= 0)
                yield break;
            
            if (Vector3.Distance(transform.position, enemyToShoot.visibilityTrigger.transform.position) > maxDistanceToAttack)
                yield break;

            if (enemyToShoot.gameObject == Game.LocalPlayer.GameObject)
            {
                var isNotInPlayerPov = !GameManager.Instance.IsPositionInPlayerFov(activeWeapon.transform.position);
                var isCoinFlippedRight = Random.value > 0.5f;
                    
                if (isNotInPlayerPov && isCoinFlippedRight)
                    yield break;
            }

            Vector3 targetDir = enemyToShoot.visibilityTrigger.transform.position - transform.position;
            float angle = Vector3.Angle(targetDir, transform.forward);
            Vector3 offset = Vector3.zero;
            if (enemyToShoot.playerMovement)
                offset = enemyToShoot.playerMovement.rb.velocity;

            if (rotateWeaponTowardTarget)
            {
                
                if (angle < minAngleToRotateGun)
                {
                    /*
                    if (handIk)
                    {
                        handIk.transform.LookAt(enemyToShoot.visibilityTrigger.transform.position + offset);
                    }
                    */
                    activeWeapon.transform.LookAt(enemyToShoot.visibilityTrigger.transform.position + offset);
                }
                else
                {
                    activeWeapon.transform.localRotation = activeWeapon.InitLocalRotation;
                }
            }

            targetDir = enemyToShoot.visibilityTrigger.transform.position - transform.position;
            angle = Vector3.Angle(targetDir, transform.forward);
                
            if (angle < minAngleToShoot)
            {
                activeWeapon.Shot(_selfUnit.HealthController, enemyToShoot.visibilityTrigger.transform);
                
                yield return new WaitForSeconds(Random.Range(weaponsAttackSwitchCooldownMinMax.x, weaponsAttackSwitchCooldownMinMax.y));
            }
        }
    }
}