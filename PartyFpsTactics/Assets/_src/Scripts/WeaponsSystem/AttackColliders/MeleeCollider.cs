using System;
using UnityEngine;

namespace MrPink.WeaponsSystem
{
    public class MeleeCollider : BaseAttackCollider
    {
        private void OnCollisionEnter(Collision collision)
        {
            // TODO implement feedback

            TryDoDamage(collision.collider);
        }
    }
}