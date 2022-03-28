using System;
using MrPink.WeaponsSystem;
using UnityEngine;

namespace MrPink.Health
{
    public class BodyPart : BasicHealth
    {
        public override CollisionTarget HandleDamageCollision(Vector3 collisionPosition, int damage, ScoringActionType actionOnHit)
        {
            UnitsManager.Instance.RagdollTileExplosion(collisionPosition, actionOnHit);

            if (_healthController == null)
                throw new Exception("BodyPart должен ссылаться на HealthController");
            
            _healthController.Damage(damage, actionOnHit, _healthController.transform);
            return CollisionTarget.Creature;
        }
    }
}