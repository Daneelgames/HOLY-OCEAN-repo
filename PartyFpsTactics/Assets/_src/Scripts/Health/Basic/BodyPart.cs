using System;
using MrPink.WeaponsSystem;
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
    }
}