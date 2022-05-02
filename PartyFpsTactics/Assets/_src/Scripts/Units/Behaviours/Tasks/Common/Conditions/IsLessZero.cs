using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Common")]
    public class IsFloatLessZero : Conditional
    {
        public SharedFloat inputValue;

        public override TaskStatus OnUpdate()
        {
            if (inputValue.Value < 0)
                return TaskStatus.Success;
            return TaskStatus.Failure;
        }
    }
}