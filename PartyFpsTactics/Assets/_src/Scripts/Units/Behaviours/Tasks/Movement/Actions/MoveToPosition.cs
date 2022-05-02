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

        public override void OnStart()
        {
            selfUnit.UnitMovement.AgentSetPath(inputPosition.Value, isFollowing);
        }

        public override TaskStatus OnUpdate()
        {
            if (Vector3.Distance(selfUnit.transform.position, inputPosition.Value) <= 1)
                return TaskStatus.Success;

            return TaskStatus.Running;
        }

    }
}