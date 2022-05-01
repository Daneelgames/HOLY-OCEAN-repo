using BehaviorDesigner.Runtime.Tasks;
using MrPink.Health;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/State")]
    public class IsAlive : BaseUnitConditional
    {
        public override TaskStatus OnUpdate()
        {
            if (selfUnit.HealthController.IsDead)
                return TaskStatus.Failure;

            return TaskStatus.Success;
        }
    }
}