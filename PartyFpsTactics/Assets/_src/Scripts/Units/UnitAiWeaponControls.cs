using System.Collections.Generic;
using MrPink.WeaponsSystem;
using UnityEngine;

namespace MrPink.Units
{
    // TODO вытащить в конфиги на SO
    public class UnitAiWeaponControls : MonoBehaviour
    {
        public List<WeaponController> activeWeapons;
        public Vector2 weaponsAttackSwitchCooldownMinMax = new Vector2(0, 0);
        public float minAngleToRotateGun = 30;
        public float minAngleToShoot = 15;
        public bool rotateWeaponTowardTarget = true;
        public float maxDistanceToAttack = 1000;
        public float maxDistanceFromPlayerToShoot = 250;
    }
}