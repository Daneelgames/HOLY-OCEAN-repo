using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units")]
    public class IsTargetNearToAttack : BaseUnitConditional
    {
        public override TaskStatus OnUpdate()
        {
            // TODO добавить синхронизированную переменную
            var distance = Vector3.Distance(
                transform.position,
                selfUnit.UnitAiMovement.enemyToLookAt.visibilityTrigger.transform.position
                );
            if (distance < selfUnit.WeaponControls.maxDistanceToAttack)
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}