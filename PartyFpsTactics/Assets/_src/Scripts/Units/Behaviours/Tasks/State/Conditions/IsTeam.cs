using BehaviorDesigner.Runtime.Tasks;
using MrPink.Health;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/State")]
    public class IsTeam : BaseUnitConditional
    {
        public Team team;
        
        public override TaskStatus OnUpdate()
        {
            if (selfUnit.HealthController.team == team)
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}