using System;
using UnityEngine;

namespace MrPink.WeaponsSystem
{
    public class MeleeCollider : BaseAttackCollider
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Коллизия с {other.gameObject.name}");
            
            var target = TryDoDamage(other);
            
            switch (target)
            {
                case CollisionTarget.Solid:
                    PlayHitSolidFeedback();
                    break;
                
                case CollisionTarget.Creature:
                    Debug.LogWarning("Can't find point");
                    break;
            }
        }
    }
}