using BehaviorDesigner.Runtime.Tasks;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units")]
    public class IsWeaponRotationNeeded : BaseUnitConditional
    {
        public override TaskStatus OnUpdate()
        {
            if (selfUnit.WeaponControls.rotateWeaponTowardTarget)
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}