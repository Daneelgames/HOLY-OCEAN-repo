using System;
using System.Collections;
using System.Net.Sockets;
using Brezg.Extensions.UniTaskExtensions;
using Cysharp.Threading.Tasks;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MrPink.WeaponsSystem
{
    public class ProjectileController : BaseAttackCollider
    {
        public bool addVelocityEveryFrame = true;
        public float projectileSpeed = 100;
    
        public ToolType toolType = ToolType.Null;
        [ShowIf("toolType", ToolType.FragGrenade)]
        [SerializeField]
        private FragGrenadeTool fragGrenadeTool;
    
        [ShowIf("toolType", ToolType.CustomLadder)]
        [SerializeField]
        private CustomLadderTool customLadderTool;
        [SerializeField]
        private ConsumableTool consumableTool;

        [SerializeField] [ReadOnly] private float currentCastRadius = 0.3f;
        [SerializeField] private float playerCastRadius = 0.5f;
        [SerializeField] private float aiCastRadius = 0.3f;
        [Space]
        
        public bool dieOnContact = true;
        public bool ricochetOnContact = false;
        public bool stickOnContact = false;
        public float ricochetCooldownMax = 0.5f;
        private float ricochetCooldown = 0;
        public Rigidbody rb;
        public float gravity = 13;
        public LayerMask solidsMask;
        public LayerMask unitsMask;
        private Vector3 currentPosition;
        private Vector3 lastPosition;
        private float distanceBetweenPositions;
        public Transform visual;
    
        public AudioSource shotAu;
        public AudioSource flyAu;
        private bool dead = false;

        private bool rbIsKinematicInit = false;
        
        private void Awake()
        {
            rbIsKinematicInit = rb.isKinematic;
        }

        void OnEnable()
        {
            dead = false;
            if (visual)
                visual.gameObject.SetActive(true);
            PlaySound(shotAu);
            PlaySound(flyAu);
        }

        public override void Init(HealthController owner, DamageSource source, Transform shotHolder, ScoringActionType action = ScoringActionType.NULL, float offsetX = 0,float offsetY = 0)
        {
            base.Init(owner, source, shotHolder, action);

            rb.isKinematic = rbIsKinematicInit;
            lastPosition = transform.position;

            if (!IsAttachedToShotHolder)
            {
                if (rb != null && !addVelocityEveryFrame)
                    rb.AddForce(transform.forward * projectileSpeed + Vector3.down * gravity, ForceMode.VelocityChange);

                transform.localEulerAngles += new Vector3(offsetX,offsetY, 0);   
            }

            StartCoroutine(UpdateLastPosition());

            CheckConsumable(owner);
        }

        void CheckConsumable(HealthController owner)
        {
            if (consumableTool)
            {
                consumableTool.Consume(owner);
                Death();
            }
            
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(lastPosition, currentPosition - lastPosition);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (dead)
                return;

            if (other.gameObject.layer != 6 && other.gameObject.layer != 11) 
                return;
        
            if (toolType != ToolType.Null && other.gameObject == Game.LocalPlayer.Movement.gameObject)
                return;

            if (stickOnContact)
                StickToObject(other);

            if (dieOnContact)
                Death();
        }
    
        private void Update()
        {
            if (dead)
                return;
            if (ricochetCooldown > 0)
                ricochetCooldown -= Time.deltaTime;
        
            if (addVelocityEveryFrame)
                rb.velocity = transform.forward * projectileSpeed + Vector3.down * gravity * Time.deltaTime;
        
            currentPosition  = transform.position;
            distanceBetweenPositions = Vector3.Distance(currentPosition, lastPosition);
            var target = CollisionTarget.Self;
            if (ownerHealth.IsPlayer)
                currentCastRadius = playerCastRadius;
            else
                currentCastRadius = aiCastRadius;
            
            if (Physics.SphereCast(lastPosition, currentCastRadius, currentPosition - lastPosition, out var hit, distanceBetweenPositions, unitsMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform == null)
                    return;
            
                if (ownerHealth != null && hit.collider.gameObject == ownerHealth.gameObject)
                    return;
                
            
                target = TryDoDamage(hit.collider);
                PlayHitUnitFeedback(hit.point);
            }
            else if (Physics.Raycast(lastPosition, currentPosition - lastPosition, out hit, distanceBetweenPositions, solidsMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform == null || (hit.collider.isTrigger && hit.transform.gameObject.layer == 11)) // ignore npc interaction colliders
                    return;
                
                target = TryDoDamage(hit.collider);
            
                switch (target)
                {
                    case CollisionTarget.Solid:
                        PlayHitSolidFeedback(transform.position);
                        break;
                
                    case CollisionTarget.Creature:
                        PlayHitUnitFeedback(hit.point);
                        break;
                }
            }
            else
                return;
        
            // DONT DAMAGE INTERACTABLE TRIGGERS AS THEY ARE ONLY FOR PLAYER INTERACTOR
            if (hit.collider.gameObject.layer == 11 && hit.collider.isTrigger)
                return;

            HandleEndOfCollision(hit, target);
        }

        private void HandleEndOfCollision(RaycastHit hit, CollisionTarget collisionTarget)
        {
            Debug.Log("projectile hit " + hit.collider.name);
            
            if (collisionTarget == CollisionTarget.Self) // THIS CONTACT DOESNT COUNT, DO NOTHING
                return;
            
            if (dieOnContact)
                Death();
            else if (ricochetOnContact)
                Ricochet(hit.normal);
            else if (stickOnContact)
                StickToObject(hit.collider);
        }
    
        private IEnumerator UpdateLastPosition()
        {
            while (true)
            {
                if (dead)
                    yield break;
            
                lastPosition = transform.position;
            
                yield return null;
            }
        }


        private void Death()
        {
            Debug.Log("Destroy projectile");
        
            if (toolType == ToolType.FragGrenade)
                fragGrenadeTool.Explode();
        
            dead = true;
            rb.isKinematic = true;
            if (visual)
                visual.gameObject.SetActive(false);
            DeathCoroutine().ForgetWithHandler();
            Release(3);
        }

        private void Ricochet(Vector3 hitNormal)
        {
            if (ricochetCooldown > 0)
                return;
        
            ricochetCooldown = ricochetCooldownMax;
            Vector3 reflectDir = Vector3.Reflect(transform.forward, hitNormal);
            transform.rotation = Quaternion.LookRotation(reflectDir);
        }

        private void StickToObject(Collider coll)
        {
            transform.parent = coll.transform;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            dead = true;
        
            if (toolType == ToolType.CustomLadder)
                customLadderTool.ConstructLadder(ownerHealth.transform.position - Vector3.up);
        }

        private async UniTask DeathCoroutine()
        {
            float t = 0;
            while (t < 0.5f)
            {
                if (flyAu == null)
                    return;
                flyAu.volume -= Time.deltaTime * 50;
                t -= Time.deltaTime;
                await UniTask.DelayFrame(1);
            }
        }
    }
}