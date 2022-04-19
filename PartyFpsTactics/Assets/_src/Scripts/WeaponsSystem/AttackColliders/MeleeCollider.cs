using System;
using MrPink.Health;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.WeaponsSystem
{
    public class MeleeCollider : BaseAttackCollider
    {
        public bool playerMeleeAttack = false;

        public bool carCrashCollider = false;
        [ShowIf("carCrashCollider", true)]
        public Rigidbody carRb;
        [ShowIf("carCrashCollider", true)]
        public float minCarRbVelocityToDamage = 50;


        private void OnTriggerEnter(Collider other)
        {
            if (carCrashCollider)
            {
                if (carRb.velocity.magnitude < minCarRbVelocityToDamage)
                    return;
            }
            if (currentLifeTime > _dangerousTime)
                return;
            if (ownerHealth == null)
                return;
            if (ownerHealth.gameObject == other.gameObject)
                return;
            
            // DONT DAMAGE INTERACTABLE TRIGGERS AS THEY ARE ONLY FOR PLAYER INTERACTOR
            if (other.gameObject.layer == 11 && other.isTrigger)
                return;

            if (other.gameObject.layer == 9 || other.gameObject.layer == 10)
            {
                // DEFLECT PROJECTILE
                if (playerMeleeAttack)
                {
                    Debug.Log("DEFLECT PROJECTILE 0");
                    var proj = other.gameObject.GetComponent<ProjectileController>();
                    if (proj /*&& proj.OwnerHealth && proj.OwnerHealth !=  Game.Player.Health*/)
                    {
                        Debug.Log("DEFLECT PROJECTILE 1");
                        proj.OwnerHealth = Game.Player.Health;
                        other.transform.Rotate(0, 180, 0, Space.Self);
                    }
                }
            }
            
            //Debug.Log($"Коллизия с {other.gameObject.name}");
            
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