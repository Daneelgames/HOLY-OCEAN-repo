using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units")]
    public class CalculateAngleToTarget : BaseUnitAction
    {
        public SharedFloat aimAngle;
        
        public override TaskStatus OnUpdate()
        {
            Vector3 targetDir = selfUnit.UnitAiMovement.enemyToLookAt.visibilityTrigger.transform.position - transform.position;
            aimAngle = Vector3.Angle(targetDir, transform.forward);
            return TaskStatus.Success;
        }
    }
}