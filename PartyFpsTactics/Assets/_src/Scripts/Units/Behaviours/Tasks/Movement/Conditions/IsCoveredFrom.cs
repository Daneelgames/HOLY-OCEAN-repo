using BehaviorDesigner.Runtime.Tasks;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/Movement")]
    public class IsCoveredFrom : BaseUnitConditional
    {
        public SharedHealthController inputEnemy;
        
        public override TaskStatus OnUpdate()
        {
            if (inputEnemy.Value == null)
                return TaskStatus.Failure;

            if (CoverSystem.IsCoveredFrom(selfUnit.HealthController, inputEnemy.Value))
                return TaskStatus.Success;
            
            return TaskStatus.Failure;
        }
    }
}