using BehaviorDesigner.Runtime.Tasks;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/Movement")]
    public class IsNoCoverSelected : BaseUnitConditional
    {
        public SharedCoverSpot selectedCoverSpot;

        public override TaskStatus OnUpdate()
        {
            if (selectedCoverSpot == null)
                return TaskStatus.Success;
            return TaskStatus.Failure;
        }
    }
}