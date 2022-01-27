using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
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
        if (other.gameObject.layer == 6)
        {
            Destroy(gameObject);
            return;
        }
        if (other.gameObject.layer == 7)
        {
            var bodyPart = other.collider.gameObject.GetComponent<BodyPart>();
            if (bodyPart)
            {
                if (bodyPart.hc == ownerHc)
                    return;
                
                bodyPart.hc.Damage(damage);
            }
            Destroy(gameObject);
        }
    }

    IEnumerator MoveProjectile()
    {
        float currentLifeTime = 0;
        while (true)
        {
            rb.velocity = transform.forward * projectileSpeed + Vector3.down * gravity * Time.deltaTime;
            Vector3 currentPosition = transform.position;
            if (Physics.Raycast(currentPosition, lastPosition - currentPosition, out var hit,
                Vector3.Distance(currentPosition, lastPosition)))
            {
                if (hit.collider.gameObject.layer == 6)
                {
                    Destroy(gameObject);
                    yield break;
                }

                if (hit.collider.gameObject.layer == 7)
                {
                    var bodyPart = hit.collider.gameObject.GetComponent<BodyPart>();
                    if (bodyPart && bodyPart.hc != ownerHc)
                    {
                        bodyPart.hc.Damage(damage);
                        Destroy(gameObject);
                        yield break;
                    }
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
    
}
