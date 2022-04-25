using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace MrPink.Units.Behaviours
{
    [TaskCategory("MrPink/Units")]
    public class SelectNextWeapon : BaseUnitAction
    {
        public SharedWeaponController nextWeapon;

        private int _currentWeaponIndex = 0;

        public override void OnBehaviorComplete()
        {
            OnIterationEnd();
        }

        public override TaskStatus OnUpdate()
        {
            Debug.Log(_currentWeaponIndex);
            
            var weaponCount = selfUnit.WeaponControls.activeWeapons.Count;

            if (_currentWeaponIndex >= weaponCount)
            {
                OnIterationEnd();
                return TaskStatus.Failure;
            }

            var weapon = selfUnit.WeaponControls.activeWeapons[_currentWeaponIndex];
            _currentWeaponIndex++;
            
            if (weapon == null)
                return OnUpdate();

            nextWeapon.Value = weapon;
            return TaskStatus.Success;
        }

        private void OnIterationEnd()
        {
            nextWeapon.Value = null;
            _currentWeaponIndex = 0;
        }
    }
}