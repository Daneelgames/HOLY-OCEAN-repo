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

    private Vector3 lastPosition;
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

    private void OnCollisionEnter(Collision other)
    {
        if (dead)
            return;
        
        if (other.gameObject == PlayerMovement.Instance.gameObject)
        {
            if (PlayerMovement.Instance.gameObject == ownerHc.gameObject)
                return;
            
            HitUnit();
            GameManager.Instance.Restart();
            Death();
            return;
        }
        
        if (other.gameObject.layer == 6)
        {
            TryToDamage(other.collider);
            HitSolid();
            Death();
            return;
        }
        
        if (other.gameObject.layer == 7)
        {
            TryToDamage(other.collider);
    
            HitUnit();
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

            rb.velocity = transform.forward * projectileSpeed + Vector3.down * gravity * Time.deltaTime;
            Vector3 currentPosition = transform.position;
            if (Physics.Raycast(currentPosition, lastPosition - currentPosition, out var hit,
                Vector3.Distance(currentPosition, lastPosition)))
            {

                if (hit.collider.gameObject == PlayerMovement.Instance.gameObject)
                {
                    HitUnit();
                    Death();
                    GameManager.Instance.Restart();
                    yield break;
                }
                
                if (hit.collider.gameObject.layer == 6)
                {
                    TryToDamage(hit.collider);
                    HitSolid();
                    Death();
                    yield break;
                }

                if (hit.collider.gameObject.layer == 7)
                {
                    TryToDamage(hit.collider);
                    HitUnit();
                    Death();
                    yield break;
                }
            }
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
    void HitUnit()
    {
        hitAu.clip = hitUnitFx;
        hitAu.pitch = Random.Range(0.75f, 1.25f);
        hitAu.Play();
        bloodParticles.parent = null;
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
