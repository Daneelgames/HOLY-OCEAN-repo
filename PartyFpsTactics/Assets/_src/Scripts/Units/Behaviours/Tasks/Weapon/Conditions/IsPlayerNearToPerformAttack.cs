using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units/Weapon")]
    public class IsPlayerNearToPerformAttack : BaseUnitConditional
    {
        public override TaskStatus OnUpdate()
        {
            // TODO добавить синхронизированную переменную
            if (Vector3.Distance(transform.position, Game.Player.MainCamera.transform.position) < selfUnit.WeaponControls.maxDistanceFromPlayerToShoot)
                return TaskStatus.Success;

            return TaskStatus.Failure;
        }
    }
}