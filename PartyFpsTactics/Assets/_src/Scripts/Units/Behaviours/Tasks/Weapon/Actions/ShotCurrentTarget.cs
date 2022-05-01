using BehaviorDesigner.Runtime.Tasks;
using Brezg.Extensions.UniTaskExtensions;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/Weapon")]
    public class ShotCurrentTarget : BaseUnitAction
    {
        public SharedWeaponController activeWeapon;
        
        public override TaskStatus OnUpdate()
        {
            // TODO держать, на кого смотрит юнит, в стейт-машине
            activeWeapon.Value.Shot(selfUnit.HealthController, selfUnit.UnitAiMovement.enemyToLookAt.visibilityTrigger.transform)
                .ForgetWithHandler();
            return TaskStatus.Success;
        }
    }
}