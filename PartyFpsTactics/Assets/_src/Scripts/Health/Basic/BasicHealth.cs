using JetBrains.Annotations;
using MrPink.WeaponsSystem;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace MrPink.Health
{
    public abstract class BasicHealth : MonoBehaviour
    {
        [SerializeField]
        [PreviouslySerializedAs("localHealth")]
        protected int _health = 100;


        [SerializeField, SceneObjectsOnly, CanBeNull]
        protected HealthController _healthController;


        public int Health
        {
            get => _healthController == null ? _health : _healthController.health;
        }


        public bool IsAlive
            => Health > 0;

        public bool IsDead
            => !IsAlive;
        

        public HealthController HealthController
        {
            set => _healthController = value;
        }


        public virtual void Damage(int damage, DamageSource source)
        {
            if (Health <= 0)
                return;
                
            if (_healthController == null)
                _health -= damage;
            else
                _healthController.Damage(damage, source);
        }


        public bool IsOwnedBy(HealthController health)
            => health != null && health == _healthController;

        
        public abstract CollisionTarget HandleDamageCollision(Vector3 collisionPosition, DamageSource source, int damage, ScoringActionType actionOnHit);


        public virtual void Kill(DamageSource source)
            => Damage(Health, source);

    }
}