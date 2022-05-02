using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/Movement")]
    public class MoveToPosition : BaseUnitAction
    {
        public SharedVector3 inputPosition;

        public bool isFollowing;
        
        private bool IsCanceled = false;

        public override void OnStart()
        {
            selfUnit.UnitMovement.AgentSetPath(inputPosition.Value, isFollowing);
            selfUnit.UnitMovement.OnTargetChange.AddListener(OnMovementCancel);
        }

        public override TaskStatus OnUpdate()
        {
            if (IsCanceled)
                return TaskStatus.Failure;

            if (Vector3.Distance(selfUnit.transform.position, inputPosition.Value) <= 1)
                return TaskStatus.Success;

            return TaskStatus.Running;
        }

        private void OnMovementCancel()
        {
            IsCanceled = true;
        }
    }
}