using System;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Threading.Tasks;

namespace MrPink.WeaponsSystem
{
    public class MeleeCollider : BaseAttackCollider
    {
        public bool canDeflectBullets = false;

        public bool carCrashCollider = false;
        [ShowIf("carCrashCollider", true)]
        public Rigidbody carRb;
        [ShowIf("carCrashCollider", true)]
        public float minCarRbVelocityToDamage = 50;

        [SerializeField] private Rigidbody ownRb;


        private void OnEnable()
        {
            ResetCollidedList();
        }
        
        private List<Collider> collidedList = new List<Collider>();

        async void ResetCollidedList()
        {
            while (true)
            {
                await Task.Delay(1000);
                collidedList.Clear();
            }
        }

        public void FollowDetachedTransform(Transform detachedParent)
        {
            Debug.Log("FollowDetachedTransform");
            ownRb.MovePosition(detachedParent.position);
            ownRb.MoveRotation(detachedParent.rotation);
        }

        private void OnTriggerStay(Collider other)
        {
            if (collidedList.Contains(other))
                return;
            
            collidedList.Add(other);
            
            if (carCrashCollider)
            {
                if (carRb == null)
                {
                    Destroy(gameObject);
                    return;
                }
                if (carRb.velocity.magnitude < minCarRbVelocityToDamage)
                    return;
            }
            if (currentLifeTime > _dangerousTime)
                return;
            
            if (ownerHealth && ownerHealth.gameObject == other.gameObject)
                return;
            
            // DONT DAMAGE INTERACTABLE TRIGGERS AS THEY ARE ONLY FOR PLAYER INTERACTOR
            if (other.gameObject.layer == 11 && other.isTrigger)
                return;

            if (other.gameObject.layer == 9 || other.gameObject.layer == 10)
            {
                // DEFLECT PROJECTILE
                if (canDeflectBullets)
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
            
            Debug.Log($" car  Коллизия с {other.gameObject.name}");
            
            var target = TryDoDamage(other);
            
            Debug.Log("car melee tryDoDamage target: " + target);
            switch (target)
            {
                case CollisionTarget.Solid:
                    PlayHitSolidFeedback(/*other.*/transform.position);
                    break;
                
                case CollisionTarget.Creature:
                    //Debug.LogWarning("Can't find point");
                    PlayHitUnitFeedback(other.transform.position);
                    break;
            }
        }
    }
}