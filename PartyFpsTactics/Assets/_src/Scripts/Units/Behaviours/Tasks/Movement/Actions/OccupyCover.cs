using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/Movement")]
    public class OccupyCover : BaseUnitAction
    {
        public SharedCoverSpot inputCoverSpot;

        public override void OnStart()
        {
            inputCoverSpot.Value.Occupator = selfUnit.HealthController;
        }

        public override TaskStatus OnUpdate()
        {
            if (inputCoverSpot == null)
                return TaskStatus.Failure;
            
            if (Vector3.Distance(transform.position, inputCoverSpot.Value.transform.position) < 1)
            {
                selfUnit.HealthController.HumanVisualController.SetInCover(true);
                transform.rotation = Quaternion.Slerp(transform.rotation, inputCoverSpot.Value.transform.rotation, Time.deltaTime * 3);
            }
            else
                selfUnit.HealthController.HumanVisualController.SetInCover(false);

            return TaskStatus.Running;
        }
    }
}