using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/Weapon")]
    public class RotateWeaponTowardTarget : BaseUnitAction
    {
        public SharedWeaponController activeWeapon;
        public SharedFloat aimAngle;

        public override TaskStatus OnUpdate()
        {
            if (!selfUnit.WeaponControls.rotateWeaponTowardTarget)
            {
                Debug.LogError("Can't rotate weapon, rotateWeaponTowardTarget is false. Tree is setup wrong?");
                return TaskStatus.Failure;
            }
            
            Vector3 offset = Vector3.zero;

            var targetMovement = selfUnit.UnitAiMovement.enemyToLookAt.playerMovement;
                
            if (targetMovement)
                offset = selfUnit.UnitAiMovement.enemyToLookAt.playerMovement.rb.velocity;
                
            if (aimAngle.Value < selfUnit.WeaponControls.minAngleToRotateGun)
                activeWeapon.Value.transform.LookAt(selfUnit.UnitAiMovement.enemyToLookAt.visibilityTrigger.transform.position + offset);
            else
                activeWeapon.Value.transform.localRotation = activeWeapon.Value.InitLocalRotation;

            return TaskStatus.Success;
        }
    }
}