using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ExplosionController : MonoBehaviour
{
    public float lifeTime = 0.2f;
    public int damage = 1000;
    public float explosionDistance = 5;
    public float explosionForce = 200;
    public float explosionForcePlayer = 100;
    public AudioSource au;
    private List<GameObject> collidedGameObjects = new List<GameObject>();
    void Start()
    {
        au.pitch = Random.Range(0.75f, 1.25f);
        au.Play();
        UnitsManager.Instance.RagdollTileExplosion(transform.position, explosionDistance, explosionForce, explosionForcePlayer);
    }

    private void Update()
    {
        if (lifeTime > 0)
            lifeTime -= Time.deltaTime;
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (lifeTime <= 0)
            return;
        if (collidedGameObjects.Contains(other.gameObject))
            return;
        
        collidedGameObjects.Add(other.gameObject);
        
        var bodyPart = other.gameObject.GetComponent<BodyPart>();
        if (bodyPart)
        {
            if (bodyPart.hc)
            {
                if (bodyPart.hc.health - damage > 0)
                    bodyPart.hc.Damage(damage);
                else
                    UnitsManager.Instance.AddBodyPartToQueue(bodyPart);
            }
            else if  (bodyPart.localHealth > 0)
            {
                if (bodyPart.localHealth - damage > 0)
                    bodyPart.DamageTile(damage);
                else
                    UnitsManager.Instance.AddBodyPartToQueue(bodyPart);
            }
                
        }
    }
}
