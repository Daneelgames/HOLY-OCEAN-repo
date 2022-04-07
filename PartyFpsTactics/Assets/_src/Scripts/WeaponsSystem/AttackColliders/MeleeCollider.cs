using System;
using MrPink.Health;
using MrPink.PlayerSystem;
using UnityEngine;

namespace MrPink.WeaponsSystem
{
    public class MeleeCollider : BaseAttackCollider
    {
        public bool playerMeleeAttack = false;
        private void OnTriggerEnter(Collider other)
        {
            if (ownerHealth == null)
                return;
            if (ownerHealth.gameObject == other.gameObject)
                return;

            if (other.gameObject.layer == 9 || other.gameObject.layer == 10)
            {
                // DEFLECT PROJECTILE
                if (playerMeleeAttack)
                {
                    Debug.Log("DEFLECT PROJECTILE 0");
                    var proj = other.gameObject.GetComponent<ProjectileController>();
                    if (proj /*&& proj.OwnerHealth && proj.OwnerHealth !=  Player.Health*/)
                    {
                        Debug.Log("DEFLECT PROJECTILE 1");
                        proj.OwnerHealth = Player.Health;
                        other.transform.Rotate(0, 180, 0, Space.Self);
                    }
                }
            }
            
            Debug.Log($"Коллизия с {other.gameObject.name}");
            
            var target = TryDoDamage(other);
            
            switch (target)
            {
                case CollisionTarget.Solid:
                    PlayHitSolidFeedback(other.transform.position);
                    break;
                
                case CollisionTarget.Creature:
                    //Debug.LogWarning("Can't find point");
                    PlayHitUnitFeedback(other.transform.position);
                    break;
            }
        }
    }
}