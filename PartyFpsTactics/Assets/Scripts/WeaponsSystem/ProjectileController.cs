using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using Random = UnityEngine.Random;

public class ProjectileController : MonoBehaviour
{
    public float projectileSpeed = 100;
    public int damage = 100;
    public float lifeTime = 2;
    public Rigidbody rb;
    private float gravity = 13;
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
    public void Init(HealthController _ownerHc)
    {
        ownerHc = _ownerHc;
        lastPosition = transform.position;
        shotAu.pitch = Random.Range(0.75f, 1.25f);
        shotAu.Play();
        flyAu.pitch = Random.Range(0.75f, 1.25f);
        flyAu.Play();
        StartCoroutine(MoveProjectile());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(lastPosition, currentPosition - lastPosition);
    }

    private void Update()
    {
        rb.velocity = transform.forward * projectileSpeed + Vector3.down * gravity * Time.deltaTime;
        currentPosition  = transform.position;
        distanceBetweenPositions = Vector3.Distance(currentPosition, lastPosition);
        if (Physics.Raycast(lastPosition, currentPosition - lastPosition, out var hit, distanceBetweenPositions, solidsMask, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.gameObject == PlayerMovement.Instance.gameObject)
            {
                HitUnit(hit.point);
                Death();
                GameManager.Instance.Restart();
                return;
            }
                
            TryToDamage(hit.collider);
            HitSolid();
            Death();
            return;
        }
        if (Physics.SphereCast(lastPosition, 0.3f, currentPosition - lastPosition, out hit, distanceBetweenPositions, unitsMask, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.gameObject == PlayerMovement.Instance.gameObject)
            {
                HitUnit(hit.point);
                Death();
                GameManager.Instance.Restart();
                return;
            }
                
            TryToDamage(hit.collider);
            HitUnit(hit.point);
            Death();
        }
    }

    void TryToDamage(Collider coll)
    {
        var bodyPart = coll.gameObject.GetComponent<BodyPart>();
        if (bodyPart)
        {
            if (bodyPart.hc == ownerHc)
                return;
                
            bodyPart.hc.Damage(damage);
        }
    }

    IEnumerator MoveProjectile()
    {
        float currentLifeTime = 0;
        while (true)
        {
            if (dead)
                yield break;

            yield return null;
            currentLifeTime += Time.deltaTime;
            if (currentLifeTime > lifeTime)
            {
                Destroy(gameObject);
                yield break;
            }
            lastPosition = transform.position;
        }
    }

    void HitSolid()
    {
        hitAu.clip = hitSolidFx;
        hitAu.pitch = Random.Range(0.75f, 1.25f);
        hitAu.Play();
        debrisParticles.parent = null;
        debrisParticles.gameObject.SetActive(true);
    }
    void HitUnit(Vector3 contactPoint)
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
