using JetBrains.Annotations;
using MrPink.Health;
using MrPink.WeaponsSystem;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace MrPink
{
    public abstract class BasicHealth : MonoBehaviour
    {
        [SerializeField]
        [PreviouslySerializedAs("localHealth")]
        private int _health = 100;


        [SerializeField, SceneObjectsOnly, CanBeNull]
        private HealthController _healthController;


        public int Health
        {
            get => _healthController == null ? _health : _healthController.health;
            protected set
            {
                if (_health <= 0)
                    return;
                
                if (_healthController == null)
                    _health = value;
                else
                    _healthController.Damage(_health + value);
            }
        }


        public bool IsAlive
            => Health > 0;

        public bool IsDead
            => !IsAlive;
        

        public HealthController HealthController
        {
            set => _healthController = value;
        }


        public virtual void Damage(int damage)
            => Health -= damage;


        public bool IsOwnedBy(HealthController health)
            => health != null && health == _healthController;

        
        public abstract CollisionTarget HandleDamageCollision(Vector3 collisionPosition, int damage, ScoringActionType actionOnHit);


        public virtual void Kill(bool combo)
            => Health = 0;

    }
}