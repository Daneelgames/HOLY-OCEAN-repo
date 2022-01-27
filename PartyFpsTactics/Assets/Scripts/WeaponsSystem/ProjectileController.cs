using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public float projectileSpeed = 100;
    public int damage = 100;
    public float lifeTime = 2;
    public Rigidbody rb;
    private float gravity = 13;

    private Vector3 lastPosition;
    private void Start()
    {
        lastPosition = transform.position;
        StartCoroutine(MoveProjectile());
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
                    if (bodyPart)
                    {
                        bodyPart.hc.Damage(damage);
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
