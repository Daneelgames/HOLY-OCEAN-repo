using System;
using MrPink.Health;
using UnityEngine;

namespace MrPink.WeaponsSystem
{
    public class MeleeCollider : BaseAttackCollider
    {
        private void OnTriggerEnter(Collider other)
        {
            if (ownerHealth == null)
                return;
            if (ownerHealth.gameObject == other.gameObject)
                return;
            
            
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