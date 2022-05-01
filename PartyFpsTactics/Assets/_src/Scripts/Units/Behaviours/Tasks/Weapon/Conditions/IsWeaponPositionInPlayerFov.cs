using BehaviorDesigner.Runtime.Tasks;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/Weapon")]
    public class IsWeaponPositionInPlayerFov : BaseUnitConditional
    {
        public SharedWeaponController activeWeapon;
        
        public override TaskStatus OnUpdate()
        {
            if (GameManager.Instance.IsPositionInPlayerFov(activeWeapon.Value.transform.position))
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}