using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using MrPink.Health;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/Vision")]
    public class GetClosestVisibleEnemy : BaseUnitAction
    {
        public SharedHealthController outputEnemy;

        private UniTask<HealthController> _task;

        public override void OnStart()
        {
            _task = selfUnit.UnitVision.GetClosestVisibleEnemy();
        }

        public override TaskStatus OnUpdate()
        {
            if (_task.Status == UniTaskStatus.Pending)
                return TaskStatus.Running;

            if (_task.Status == UniTaskStatus.Faulted)
                return TaskStatus.Failure;

            if (_task.Status == UniTaskStatus.Canceled)
                return TaskStatus.Failure;

            outputEnemy.Value = _task.GetAwaiter().GetResult();

            if (outputEnemy.Value == null)
                return TaskStatus.Failure;

            return TaskStatus.Success;
        }
        
        
    }
}