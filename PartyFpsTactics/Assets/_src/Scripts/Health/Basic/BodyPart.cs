using MrPink.WeaponsSystem;
using UnityEngine;

namespace MrPink.Health
{
    public class BodyPart : BasicHealth
    {
        public override CollisionTarget HandleDamageCollision(Vector3 collisionPosition, int damage, ScoringActionType actionOnHit)
        {
            throw new System.NotImplementedException();
        }
    }
}