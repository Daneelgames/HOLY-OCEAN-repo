using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MrPink.WeaponsSystem
{
    public class BaseAttackCollider : MonoBehaviour
    {
        [Tooltip("For Vehicle Damage Colliders")]
        public bool autoInitOnStart = false;
        [SerializeField]
        [Range(0, 1000)]
        protected int damage = 20;

        
        [SerializeField] 
        private bool _isSelfCollisionAvailable = true;

        [SerializeField] 
        private bool _isPlayerCollisionAvailable = true;
        

        [SerializeField] 
        private bool _isAttachedToShotHolder = false;
        

        [Header("If lifetime < 0, this object will not die on timer")]
        [FormerlySerializedAs("lifeTime")]
        [SerializeField]
        private float _lifeTime = 2;
        [SerializeField] internal float currentLifeTime = 0;
        [SerializeField] internal float _dangerousTime = 2;

        [SerializeField] private float ragdollExplosionDistance = 2;
        [SerializeField] private float ragdollExplosionForce = 500;
        [SerializeField] private float playerExplosionForce = 100;
        private bool unitsExplosionCompleted = false;
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
        
        [SerializeField]
        private protected HealthController ownerHealth;
        public HealthController OwnerHealth
        {
            get => ownerHealth;
            set => ownerHealth = value;
        }

        protected ScoringActionType actionOnHit;

        private DamageSource _damageSource;

        private List<HealthController> damagedHealthControllers = new List<HealthController>();
        public bool IsAttachedToShotHolder
            => _isAttachedToShotHolder;

        public float LifeTime
            => _lifeTime;


        private void Start()
        {
            if (autoInitOnStart)
            {
                actionOnHit = ScoringActionType.NULL;
                _damageSource = DamageSource.Environment;
                StartCoroutine(LifetimeCoroutine());
            }
        }

        public virtual void Init(HealthController owner, DamageSource source,  ScoringActionType action = ScoringActionType.NULL)
        {
            ownerHealth = owner;
            actionOnHit = action;
            _damageSource = source;

            StartCoroutine(LifetimeCoroutine());
        }

        void UnitsExplosion()
        {
            if (!unitsExplosionCompleted)
            {
                InteractableEventsManager.Instance.ExplosionNearInteractables(transform.position);
                UnitsManager.Instance.RagdollTileExplosion(transform.position, ragdollExplosionDistance,
                    ragdollExplosionForce, playerExplosionForce);
                unitsExplosionCompleted = true;
            }
        }
        
        protected CollisionTarget TryDoDamage(Collider targetCollider, float damageScaler = 1)
        {
            // DONT DAMAGE INTERACTABLE TRIGGERS AS THEY ARE ONLY FOR PLAYER INTERACTOR
            if (targetCollider.gameObject.layer == 11 && targetCollider.isTrigger)
            {
                Debug.Log("return CollisionTarget.Self;");
                return CollisionTarget.Self;
            }
            
            if (currentLifeTime > _dangerousTime)
            {
                Debug.Log("return CollisionTarget.Self;");
                return CollisionTarget.Self;
            }
            
            if (!_isSelfCollisionAvailable && ownerHealth && ownerHealth.gameObject == targetCollider.gameObject)
            {
                Debug.Log("return CollisionTarget.Self;");
                return CollisionTarget.Self;
            }

            if (!_isPlayerCollisionAvailable && targetCollider.gameObject == Game.Player.Movement.gameObject)
            {
                Debug.Log("return CollisionTarget.Self;");
                return CollisionTarget.Self;
            }

            
            var resultDmg = Mathf.RoundToInt(damage * damageScaler);
                
            /*
            if (damageScaler > 1)
                Debug.Log("TileAttack damageScaler " + damageScaler);*/
            
            
            if (targetCollider.gameObject == Game.Player.GameObject && IsPlayerEnemyToOwner())
            {
                if (ownerHealth.controlledMachine &&
                    ownerHealth.controlledMachine == Game.Player.VehicleControls.controlledMachine)
                {
                    Debug.Log("return CollisionTarget.Self;");
                    return CollisionTarget.Self;
                }
                
                Game.Player.Health.Damage(resultDmg, _damageSource, actionOnHit);
                if (ownerHealth.team == Game.Player.Health.team || ownerHealth.team == Team.NULL)
                    ownerHealth.UnitVision.ForgiveUnit(Game.Player.Health);
                UnitsExplosion();
                Debug.Log("return CollisionTarget.Creature;");
                return CollisionTarget.Creature;
            }

            var targetHealth = targetCollider.gameObject.GetComponent<BasicHealth>();
            
            if (targetHealth == null)
            {
                if (targetCollider.isTrigger)
                {
                    Debug.Log("return CollisionTarget.Self;");
                    return CollisionTarget.Self;
                }
                
                UnitsExplosion();
                return CollisionTarget.Solid;
            }

            if (ownerHealth && ownerHealth != Game.Player.Health)
            {
                // if vehicle tries to damage unit inside
                if (targetHealth.HealthController && targetHealth.HealthController.aiVehicleControls &&
                    ownerHealth.controlledMachine == targetHealth.HealthController.aiVehicleControls.controlledMachine)
                {
                    //Debug.Log("return CollisionTarget.Self;");
                    return CollisionTarget.Self;
                }

                // if unit inside tries to hit the vehicle AND IT'S NOT PLAYER
                if (targetHealth.HealthController && targetHealth.HealthController.controlledMachine &&
                    ownerHealth.aiVehicleControls && ownerHealth.aiVehicleControls.controlledMachine != null &&  ownerHealth.aiVehicleControls.controlledMachine ==
                    targetHealth.HealthController.controlledMachine)
                {
                    //Debug.Log("return CollisionTarget.Self;");
                    return CollisionTarget.Self;
                }
            }

            if (targetHealth.IsOwnedBy(ownerHealth))
            {
                Debug.Log("return CollisionTarget.Self;");
                return CollisionTarget.Self;
            }

            if (targetHealth.HealthController)
            {
                if (damagedHealthControllers.Count > 0 && damagedHealthControllers.Contains(targetHealth.HealthController))
                {
                    Debug.Log("return CollisionTarget.Creature;");
                    return CollisionTarget.Creature;
                }

                if (ownerHealth && damage > 0)
                {
                    Debug.Log("SetDamager; damage  " + damage);
                    targetHealth.HealthController.SetDamager(ownerHealth);
                }
                    
                Debug.Log("Damage " + targetHealth.HealthController);
                damagedHealthControllers.Add(targetHealth.HealthController);
                
                if (ownerHealth && ownerHealth.UnitVision && 
                    (ownerHealth.team == targetHealth.HealthController.team || targetHealth.HealthController.team == Team.NULL))
                    ownerHealth.UnitVision.ForgiveUnit(targetHealth.HealthController);
                
                StartCoroutine(ClearDamagedHC(targetHealth.HealthController));
            }

            // reduce velocity of vehicle who did the damage
            if (ownerHealth && ownerHealth.controlledMachine)
            {
                resultDmg = Mathf.RoundToInt(resultDmg * ownerHealth.controlledMachine.rb.velocity.magnitude);
                if (targetHealth.gameObject.layer == 6 || targetHealth.gameObject.layer == 12)
                    ownerHealth.controlledMachine.AddForceOnImpact(targetCollider.bounds.center);
            }
            
            UnitsExplosion();
            
            if (targetHealth.HealthController && targetHealth.HealthController.team == Team.Red)
                Debug.Log("DAMAGE RED FOR " + resultDmg + " DAMAGE");
            return targetHealth.HandleDamageCollision(transform.position, _damageSource, resultDmg, actionOnHit);
        }

        bool IsPlayerEnemyToOwner()
        {
            if (!ownerHealth)
                return true; // damage anyway

            if (ownerHealth.team != Game.Player.Health.team)
                return true;
            
            
            if (ownerHealth.UnitVision._enemiesToRemember.Contains(Game.Player.Health))
                return true;

            return false;
        }
        
        IEnumerator ClearDamagedHC(HealthController hc)
        {
            yield return new WaitForSeconds(1);
            for (int i = damagedHealthControllers.Count - 1; i >= 0; i--)
            {
                if (damagedHealthControllers[i] == hc)
                    damagedHealthControllers.RemoveAt(i);
            }
        }

        protected void PlaySound([CanBeNull] AudioSource source)
        {
            if (source == null) 
                return;
            
            source.pitch = Random.Range(0.75f, 1.25f);
            source.Play();
        }

        // TODO вытащить в отдельный компонент
        private float playSoundCooldownCurrent = 0;
        protected void PlaySound([CanBeNull] AudioSource source, [CanBeNull] AudioClip clip)
        {
            if (playSoundCooldownCurrent > 0)
                return;
            StartCoroutine(PlaySoundCooldown());
            
            if (source == null) 
                return;
            
            if (clip == null) 
                return;

            source.clip = clip;
            PlaySound(source);
        }

        IEnumerator PlaySoundCooldown()
        {
            playSoundCooldownCurrent = 0.25f;
            yield return new WaitForSeconds(playSoundCooldownCurrent);
            playSoundCooldownCurrent = 0;
        }
        
        protected void PlayHitSolidFeedback(Vector3 point)
        {
            PlaySound(_hitAudioSource, _hitSolidFx);
            
            if (_debrisParticles == null)
                return;

            var newParticles = Instantiate(_debrisParticles);
            newParticles.parent = null;
            newParticles.position = point;
            newParticles.localScale = Vector3.one;
            newParticles.gameObject.SetActive(true);
            /*
            _debrisParticles.parent = null;
            _debrisParticles.gameObject.SetActive(true);*/
        }
    
        protected void PlayHitUnitFeedback(Vector3 contactPoint)
        {
            PlaySound(_hitAudioSource, _hitUnitFx);
            
            if (_bloodParticles == null)
                return;
            
            var newParticles = Instantiate(_bloodParticles);
            newParticles.parent = null;
            newParticles.localScale = Vector3.one;
            newParticles.position = contactPoint;
            newParticles.gameObject.SetActive(true);
            
            /*
            _bloodParticles.parent = null;
            _bloodParticles.position = contactPoint;
            _bloodParticles.gameObject.SetActive(true);*/
        }
        
        private IEnumerator LifetimeCoroutine()
        {
            float t = 0;
            float tt = 0.5f;
            while (true)
            {
                yield return null;

                if (unitsExplosionCompleted)
                {
                    t += Time.deltaTime;
                    if (t >= tt)
                    {
                        t = 0;
                        unitsExplosionCompleted = false;
                    }
                }
                
                if (_lifeTime <= 0)
                {
                    continue;
                }
            
                currentLifeTime += Time.deltaTime;

                if (currentLifeTime > _lifeTime)
                {
                    Destroy(gameObject);
                    yield break;
                }

            }
            
        }
    }
}