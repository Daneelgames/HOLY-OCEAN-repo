using BehaviorDesigner.Runtime.Tasks;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/Weapon")]
    public class IsWeaponOnCooldown : BaseUnitConditional
    {
        public SharedWeaponController activeWeapon;
        
        public override TaskStatus OnUpdate()
        {
            if (activeWeapon.Value.OnCooldown)
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}