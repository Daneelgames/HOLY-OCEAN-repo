using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units")]
    public class IsLookingAtPlayer : BaseUnitConditional
    {
        public override TaskStatus OnUpdate()
        {
            if (selfUnit.UnitAiMovement.enemyToLookAt.gameObject == Game.Player.GameObject)
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}