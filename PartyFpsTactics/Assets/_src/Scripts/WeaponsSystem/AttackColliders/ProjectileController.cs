using System.Collections;
using Brezg.Extensions.UniTaskExtensions;
using Cysharp.Threading.Tasks;
using MrPink.Health;
using MrPink.Tools;
using MrPink.WeaponsSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class ProjectileController : BaseAttackCollider
{
    public bool addVelocityEveryFrame = true;
    public float projectileSpeed = 100;
    [Header("If lifetime < 0, this object will not die on timer")]
    public float lifeTime = 2;

    public ToolType toolType = ToolType.Null;
    
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
    
    public AudioSource shotAu;
    public AudioSource flyAu;
    private bool dead = false;
    
    [ShowIf("toolType", ToolType.FragGrenade)]
    [SerializeField]
    private FragGrenade _fragGrenade;
    
    [ShowIf("toolType", ToolType.CustomLadder)]
    [SerializeField]
    private CustomLadder _customLadder;
    
    
    public override void Init(HealthController owner, DamageSource source, ScoringActionType action = ScoringActionType.NULL)
    {
        base.Init(owner, source, action);

        lastPosition = transform.position;
        
        PlaySound(shotAu);
        PlaySound(flyAu);
        
        if (rb != null && !addVelocityEveryFrame)
            rb.AddForce(transform.forward * projectileSpeed + Vector3.down * gravity, ForceMode.VelocityChange);
        
        StartCoroutine(MoveProjectile());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(lastPosition, currentPosition - lastPosition);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (dead)
            return;

        if (other.gameObject.layer != 6) 
            return;
        
        
        if (stickOnContact)
            StickToObject(other.collider);

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
        
        if (Physics.Raycast(lastPosition, currentPosition - lastPosition, out var hit, distanceBetweenPositions, solidsMask, QueryTriggerInteraction.Collide))
        {
            if (hit.transform == null)
                return;
                
            var target = TryDoDamage(hit.collider);
            
            switch (target)
            {
                case CollisionTarget.Solid:
                    PlayHitSolidFeedback();
                    break;
                
                case CollisionTarget.Creature:
                    PlayHitUnitFeedback(hit.point);
                    break;
            }
        }
        else if (Physics.SphereCast(lastPosition, 0.3f, currentPosition - lastPosition, out hit, distanceBetweenPositions, unitsMask, QueryTriggerInteraction.Collide))
        {
            if (hit.transform == null)
                return;
            
            if (ownerHealth != null && hit.collider.gameObject == ownerHealth.gameObject)
                return;
                
            TryDoDamage(hit.collider);
            PlayHitUnitFeedback(hit.point);
        }
        else
            return;
        
        HandleEndOfCollision(hit);
    }

    private void HandleEndOfCollision(RaycastHit hit)
    {
        if (dieOnContact)
            Death();
        else if (ricochetOnContact)
            Ricochet(hit.normal);
        else if (stickOnContact)
            StickToObject(hit.collider);
    }
    
    private IEnumerator MoveProjectile()
    {
        float currentLifeTime = 0;
        while (true)
        {
            if (dead)
                yield break;

            yield return null;
            if (lifeTime > 0)
            {
                currentLifeTime += Time.deltaTime;

                if (currentLifeTime > lifeTime)
                {
                    Destroy(gameObject);
                    yield break;
                }
            }
            
            lastPosition = transform.position;
        }
    }
    

    private void Death()
    {
        Debug.Log("Destroy projectile");
        
        if (toolType == ToolType.FragGrenade)
            _fragGrenade.Explode();
        
        dead = true;
        rb.isKinematic = true;
        transform.GetChild(0).gameObject.SetActive(false);
        DeathCoroutine().ForgetWithHandler();
        Destroy(gameObject, 3);
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
            _customLadder.ConstructLadder(ownerHealth.transform.position);
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
