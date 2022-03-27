using JetBrains.Annotations;
using MrPink.Health;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.WeaponsSystem
{
    public class BaseAttackCollider : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 1000)]
        protected int damage = 100;

        
        [SerializeField] 
        private bool _isSelfCollisionAvailable = true;


        [SerializeField, AssetsOnly, CanBeNull]
        [BoxGroup("Саунд")]
        private AudioClip _hitSolidFx;
        
        
        [SerializeField, AssetsOnly, CanBeNull]
        [BoxGroup("Саунд")]
        private AudioClip _hitUnitFx;
        
        
        [SerializeField, ChildGameObjectsOnly, CanBeNull]
        [BoxGroup("Саунд")]
        private AudioSource _hitAudioSource;
        
        
        [SerializeField, ChildGameObjectsOnly, CanBeNull]
        [BoxGroup("Частицы")]
        private Transform _debrisParticles;
        
        
        [SerializeField, ChildGameObjectsOnly, CanBeNull]
        [BoxGroup("Частицы")]
        private Transform _bloodParticles;
        
        
        protected HealthController ownerHealth;
        
        protected ScoringActionType actionOnHit;

        
        public virtual void Init(HealthController owner, ScoringActionType action = ScoringActionType.NULL)
        {
            ownerHealth = owner;
            actionOnHit = action;
        }

        // 0 - solid, 1 - unit
        protected CollisionTarget TryDoDamage(Collider coll)
        {
            if (!_isSelfCollisionAvailable && ownerHealth.gameObject == coll.gameObject)
                return CollisionTarget.Self;

            Debug.Log($"Colliding with {coll.name}");

            
            if (coll.gameObject == Player.GameObject)
            {
                Player.Health.Damage(damage, actionOnHit);
                return CollisionTarget.Creature;
            }

            var bodyPart = coll.gameObject.GetComponent<BodyPart>();

            if (bodyPart == null)
                return CollisionTarget.Solid;
            
            if (bodyPart.hc == ownerHealth)
                return CollisionTarget.Self;
            
            if (bodyPart.hc == null)
            {
                if (bodyPart.localHealth <= 0) 
                    return CollisionTarget.Solid;
                
                UnitsManager.Instance.RagdollTileExplosion(transform.position, actionOnHit);
                bodyPart.DamageTile(damage, actionOnHit);
            }
            else
            {
                UnitsManager.Instance.RagdollTileExplosion(transform.position, actionOnHit);
                bodyPart.hc.Damage(damage, actionOnHit, ownerHealth.transform);
            }

            return CollisionTarget.Solid;
        }

        protected void PlaySound([CanBeNull] AudioSource source)
        {
            if (source == null) 
                return;
            
            source.pitch = Random.Range(0.75f, 1.25f);
            source.Play();
        }

        // TODO вытащить в отдельный компонент
        protected void PlaySound([CanBeNull] AudioSource source, [CanBeNull] AudioClip clip)
        {
            if (source == null) 
                return;
            
            if (clip == null) 
                return;

            source.clip = clip;
            PlaySound(source);
        }
        
        protected void PlayHitSolidFeedback()
        {
            PlaySound(_hitAudioSource, _hitSolidFx);
            
            if (_debrisParticles == null)
                return;
            _debrisParticles.parent = null;
            _debrisParticles.gameObject.SetActive(true);
        }
    
        protected void PlayHitUnitFeedback(Vector3 contactPoint)
        {
            PlaySound(_hitAudioSource, _hitUnitFx);
            
            if (_bloodParticles == null)
                return;
            _bloodParticles.parent = null;
            _bloodParticles.position = contactPoint;
            _bloodParticles.gameObject.SetActive(true);
        }
    }
}