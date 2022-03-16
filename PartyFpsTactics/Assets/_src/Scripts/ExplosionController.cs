using System.Collections.Generic;
using MrPink.PlayerSystem;
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
    private ScoringActionType scoringAction = ScoringActionType.NULL;
    
    public void Init(ScoringActionType action)
    {
        scoringAction = action;
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
        
        if (Vector3.Distance(transform.position,Player.GameObject.transform.position) <= explosionDistance)
        {
            collidedGameObjects.Add(Player.Movement.gameObject);
            Player.Health.Damage(damage);
            return;
        }
        
        var bodyPart = other.gameObject.GetComponent<BodyPart>();
        if (bodyPart)
        {
            if (bodyPart.hc)
            {
                if (bodyPart.hc.health - damage > 0)
                    bodyPart.hc.Damage(damage);
                else
                    UnitsManager.Instance.AddBodyPartToQueue(bodyPart, scoringAction);
            }
            else if  (bodyPart.localHealth > 0)
            {
                if (bodyPart.localHealth - damage > 0)
                    bodyPart.DamageTile(damage);
                else
                    UnitsManager.Instance.AddBodyPartToQueue(bodyPart, scoringAction);
            }
                
        }
    }
}
