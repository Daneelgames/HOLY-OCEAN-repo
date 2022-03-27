using JetBrains.Annotations;
using MrPink.Health;
using MrPink.PlayerSystem;
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
        
        protected HealthController ownerHealth;
        
        protected ScoringActionType actionOnHit;

        
        public virtual void Init(HealthController owner, ScoringActionType action = ScoringActionType.NULL)
        {
            ownerHealth = owner;
            actionOnHit = action;
        }

        protected int TryDoDamage(Collider coll)
        {
            if (!_isSelfCollisionAvailable && ownerHealth.gameObject == coll.gameObject)
                return 0;
            
            Debug.Log($"Colliding with {coll.name}");
            
            int damagedObjectType = 0;// 0 - solid, 1 - unit
            
            if (coll.gameObject == Player.GameObject)
            {
                Player.Health.Damage(damage, actionOnHit);
                return 1;
            }
            
            
            var bodyPart = coll.gameObject.GetComponent<BodyPart>();
            if (bodyPart)
            {
                if (bodyPart.hc == null && bodyPart.localHealth > 0)
                {
                    UnitsManager.Instance.RagdollTileExplosion(transform.position, actionOnHit);
                    bodyPart.DamageTile(damage, actionOnHit);
                    damagedObjectType = 0;
                }
                if (bodyPart.hc == ownerHealth)
                    return -1;
                
                if (bodyPart && bodyPart.hc)
                {
                    UnitsManager.Instance.RagdollTileExplosion(transform.position, actionOnHit);
                    bodyPart.hc.Damage(damage, actionOnHit, ownerHealth.transform);
                }
            }

            return damagedObjectType;
        }
        
        protected void PlaySound([CanBeNull] AudioSource source)
        {
            if (source == null) 
                return;
            
            source.pitch = Random.Range(0.75f, 1.25f);
            source.Play();
        }
    }
}