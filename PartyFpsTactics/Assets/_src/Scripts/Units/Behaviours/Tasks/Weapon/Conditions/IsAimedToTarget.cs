using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/Weapon")]
    public class IsAimedToTarget : BaseUnitConditional
    {
        public SharedFloat aimAngle;
        
        public override TaskStatus OnUpdate()
        {
            if (aimAngle.Value < selfUnit.WeaponControls.minAngleToShoot)
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}