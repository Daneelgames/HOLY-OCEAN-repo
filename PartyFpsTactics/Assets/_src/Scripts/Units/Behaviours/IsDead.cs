using BehaviorDesigner.Runtime.Tasks;

namespace MrPink.Units.Behaviours
{
    public class IsDead : Conditional
    {
        private Unit _selfUnit;
        
        public override void OnAwake()
        {
            _selfUnit = GetComponent<Unit>();
        }
        
        public override TaskStatus OnUpdate()
        {
            if (_selfUnit.HealthController.IsDead)
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}