using BehaviorDesigner.Runtime.Tasks;
using MrPink.Health;

namespace MrPink.Units.Behaviours
{
    public class IsAlive : Conditional
    {
        private Unit _selfUnit;
        
        public override void OnAwake()
        {
            _selfUnit = GetComponent<Unit>();
        }

        public override TaskStatus OnUpdate()
        {
            if (_selfUnit.HealthController.IsDead)
                return TaskStatus.Failure;

            return TaskStatus.Success;
        }
    }
}