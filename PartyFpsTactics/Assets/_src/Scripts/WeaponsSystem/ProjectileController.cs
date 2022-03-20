using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProjectileController : MonoBehaviour
{
    public bool addVelocityEveryFrame = true;
    public float projectileSpeed = 100;
    public int damage = 100;
    [Header("If lifetime < 0, this object will not die on timer")]
    public float lifeTime = 2;

    public ToolType toolType = ToolType.Null;

    [ShowIf("toolType", ToolType.CustomLadder)]
    public CustomLadder customLadder;
    
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
    private HealthController ownerHc;
    public AudioSource shotAu;
    public AudioSource flyAu;
    public AudioClip hitSolidFx;
    public AudioClip hitUnitFx;
    public AudioSource hitAu;
    private bool dead = false;

    public Transform debrisParticles;
    public Transform bloodParticles;

    private ScoringActionType actionOnHit = ScoringActionType.NULL;
    
    public void Init(HealthController _ownerHc, ScoringActionType action = ScoringActionType.NULL)
    {
        ownerHc = _ownerHc;

        actionOnHit = action;
        
        lastPosition = transform.position;
        shotAu.pitch = Random.Range(0.75f, 1.25f);
        shotAu.Play();
        flyAu.pitch = Random.Range(0.75f, 1.25f);
        flyAu.Play();
        
        if (!addVelocityEveryFrame)
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
        
        if (other.gameObject.layer == 6 && stickOnContact)
        {
            StickToObject(other.collider);
        }
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
                
            var type = TryToDamage(hit.collider); // 0 solid, 1 unit
            
            if (type == 0) HitSolidFeedback();
            else HitUnitFeedback(hit.point);
            
            if (dieOnContact)
                Death();
            else if (ricochetOnContact)
            {
                Ricochet(hit.normal);
            }
            else if (stickOnContact)
            {
                StickToObject(hit.collider);
            }
            return;
        }
        if (Physics.SphereCast(lastPosition, 0.3f, currentPosition - lastPosition, out hit, distanceBetweenPositions, unitsMask, QueryTriggerInteraction.Collide))
        {
            if (hit.transform == null)
                return;
            
            if (ownerHc != null && hit.collider.gameObject == ownerHc.gameObject)
                return;
                
            TryToDamage(hit.collider);
            HitUnitFeedback(hit.point);
            if (dieOnContact)
                Death();
            else if (ricochetOnContact)
            {
                Ricochet(hit.normal);
            }
            else if (stickOnContact)
            {
                StickToObject(hit.collider);
            }
        }
    }

    int TryToDamage(Collider coll)
    {
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
            if (bodyPart.hc == ownerHc)
                return -1;
                
            if (bodyPart && bodyPart.hc)
            {
                UnitsManager.Instance.RagdollTileExplosion(transform.position, actionOnHit);
                bodyPart.hc.Damage(damage, actionOnHit, ownerHc.transform);
            }
        }

        return damagedObjectType;
    }

    IEnumerator MoveProjectile()
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

    void HitSolidFeedback()
    {
        hitAu.clip = hitSolidFx;
        hitAu.pitch = Random.Range(0.75f, 1.25f);
        hitAu.Play();
        debrisParticles.parent = null;
        debrisParticles.gameObject.SetActive(true);
    }
    void HitUnitFeedback(Vector3 contactPoint)
    {
        hitAu.clip = hitUnitFx;
        hitAu.pitch = Random.Range(0.75f, 1.25f);
        hitAu.Play();
        bloodParticles.parent = null;
        bloodParticles.position = contactPoint;
        bloodParticles.gameObject.SetActive(true);
    }

    void Death()
    {
        dead = true;
        rb.isKinematic = true;
        transform.GetChild(0).gameObject.SetActive(false);
        StartCoroutine(DeathCoroutine());
        Destroy(gameObject, 3);
    }

    void Ricochet(Vector3 hitNormal)
    {
        if (ricochetCooldown > 0)
            return;
        
        ricochetCooldown = ricochetCooldownMax;
        Vector3 reflectDir = Vector3.Reflect(transform.forward, hitNormal);
        transform.rotation = Quaternion.LookRotation(reflectDir);
    }

    public void StickToObject(Collider coll)
    {
        transform.parent = coll.transform;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        dead = true;
        customLadder.ConstructLadder(ownerHc.transform.position);
    }

    IEnumerator DeathCoroutine()
    {
        float t = 0;
        while (t < 0.5f)
        {
            flyAu.volume -= Time.deltaTime * 50;
            yield return null;
        }
    }
}
