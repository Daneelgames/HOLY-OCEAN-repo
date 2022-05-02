using BehaviorDesigner.Runtime.Tasks;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/Movement")]
    public class CheckNoCoverBehaviour : BaseUnitConditional
    {
        public EnemiesBehaviour desiredNoCoverBehaviour;
        
        public override TaskStatus OnUpdate()
        {
            if (selfUnit.UnitAiMovement.noCoverBehaviour == desiredNoCoverBehaviour)
                return TaskStatus.Success;
            return TaskStatus.Failure;
        }
    }
}