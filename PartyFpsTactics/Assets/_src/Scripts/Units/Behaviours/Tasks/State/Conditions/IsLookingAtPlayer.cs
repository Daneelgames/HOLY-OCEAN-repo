using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/State")]
    public class IsLookingAtPlayer : BaseUnitConditional
    {
        public override TaskStatus OnUpdate()
        {
            if (selfUnit.UnitAiMovement.enemyToLookAt == null)
                return TaskStatus.Failure;
            
            if (Game.Player == null)
                return TaskStatus.Failure;
            
            if (selfUnit.UnitAiMovement.enemyToLookAt.gameObject != Game.Player.GameObject)
                return TaskStatus.Failure;

            return TaskStatus.Success;
        }
    }
}