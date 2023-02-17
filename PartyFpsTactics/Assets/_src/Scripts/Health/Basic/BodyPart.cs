using System;
using MrPink.WeaponsSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.Health
{
    public class BodyPart : BasicHealth
    {
        [Range(0.1f, 10f)]
        [SerializeField] private float damageScaler = 1;
        public override CollisionTarget HandleDamageCollision(Vector3 collisionPosition, DamageSource source, int damage, ScoringActionType actionOnHit)
        {
            //UnitsManager.Instance.RagdollTileExplosion(collisionPosition, actionOnHit);

            if (_healthController == null)
            {
                //throw new Exception("BodyPart должен ссылаться на HealthController. Имя: " + gameObject.name);
                return CollisionTarget.Self;
            }
            
            _healthController.Damage(Mathf.RoundToInt(damage * damageScaler), source, actionOnHit, _healthController.transform);
            return CollisionTarget.Creature;
        }

        [SerializeField] private bool climbable = false;
        [ShowIf("climbable")][SerializeField] private Rigidbody rb;

        private void OnTriggerEnter(Collider other)
        {
            if (climbable == false)
                return;
            
            if (Game.LocalPlayer == null) return;
            
            if (other.gameObject != Game.LocalPlayer.gameObject)
                return;
            
            Game.LocalPlayer.Movement.SetMovingPlatform(rb);
        }
        private void OnTriggerExit(Collider other)
        {
            if (climbable == false)
                return;
            
            if (Game.LocalPlayer == null) return;
            
            if (other.gameObject != Game.LocalPlayer.gameObject)
                return;
            
            Game.LocalPlayer.Movement.SetMovingPlatform(rb, true);
        }

        [Button]
        public void GetRb()
        {
            rb = gameObject.GetComponent<Rigidbody>();
        }
    }
}