using System;
using UnityEngine;

namespace MrPink.WeaponsSystem
{
    public class MeleeCollider : BaseAttackCollider
    {
        private void OnTriggerEnter(Collider other)
        {
            // TODO implement feedback

            TryDoDamage(other);
        }
    }
}