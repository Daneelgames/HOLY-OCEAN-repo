using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units")]
    public class WaitWeaponSwitchCooldown : BaseUnitAction
    {
        private float _currentTime;
        private float _waitTime;

        public override void OnStart()
        {
            _currentTime = 0;
            _waitTime = Random.Range(
                selfUnit.WeaponControls.weaponsAttackSwitchCooldownMinMax.x,
                selfUnit.WeaponControls.weaponsAttackSwitchCooldownMinMax.y
            );
        }

        public override TaskStatus OnUpdate()
        {
            _currentTime += Time.deltaTime;

            if (_currentTime < _waitTime)
                return TaskStatus.Running;
            return TaskStatus.Success;
        }
    }
}