using System;
using BehaviorDesigner.Runtime;
using MrPink.WeaponsSystem;

namespace MrPink.Units.Behaviours
{
    [Serializable]
    public class SharedWeaponController : SharedVariable<WeaponController>
    {
        public static implicit operator SharedWeaponController(WeaponController value) 
            => new SharedWeaponController { Value = value };
    }
}