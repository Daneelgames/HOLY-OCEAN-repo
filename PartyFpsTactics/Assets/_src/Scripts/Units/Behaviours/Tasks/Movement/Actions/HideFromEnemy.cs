using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/Movement")]
    public class HideFromEnemy : BaseUnitAction
    {
        public SharedHealthController inputEnemy;
        public SharedVector3 outputPosition;

        public override TaskStatus OnUpdate()
        {
            outputPosition.Value =
                transform.position + (inputEnemy.Value.transform.position - transform.position).normalized * 5;

            return TaskStatus.Success;
        }
    }
}