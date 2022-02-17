using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsManager : MonoBehaviour
{
    public static UnitsManager Instance;
    public List<HealthController> unitsInGame = new List<HealthController>();

    public float tileExplosionDistance = 3;
    public float tileExplosionForce = 100;
    public float tileExplosionForceBarrels = 50;
    public float tileExplosionForcePlayer = 100;

    public List<BodyPart> bodyPartsQueueToKill = new List<BodyPart>();
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(BodyPartsKillQueue());
    }

    public void RagdollTileExplosion(Vector3 explosionPosition, ScoringSystem.ActionType action)
    {
        RagdollTileExplosion(explosionPosition, -1, -1, -1, action);
    }
    public void RagdollTileExplosion(Vector3 explosionPosition, float distance = -1, float force = -1, float playerForce = -1, ScoringSystem.ActionType action = ScoringSystem.ActionType.NULL)
    {
        if (distance < 0)
            distance = tileExplosionDistance;
        if (force < 0)
            force = tileExplosionForce;
        if (playerForce < 0)
            playerForce = tileExplosionForcePlayer;
        
        for (int i = 0; i < unitsInGame.Count; i++)
        {
            if (Vector3.Distance(explosionPosition, unitsInGame[i].transform.position + Vector3.up) <= distance)
            {
                if (unitsInGame[i].playerMovement)
                {
                    unitsInGame[i].playerMovement.rb
                        .AddForce((unitsInGame[i].visibilityTrigger.transform.position - explosionPosition).normalized * playerForce, ForceMode.VelocityChange);
                    continue;
                }
                if (unitsInGame[i].rb)
                {
                    unitsInGame[i].rb.AddForce((unitsInGame[i].visibilityTrigger.transform.position - explosionPosition).normalized *
                                               tileExplosionForceBarrels, ForceMode.VelocityChange);
                    
                    if (action != ScoringSystem.ActionType.NULL)
                        ScoringSystem.Instance.RegisterAction(ScoringSystem.ActionType.BarrelBumped);
                }

                if (unitsInGame[i].HumanVisualController)
                {
                    if (action != ScoringSystem.ActionType.NULL)
                        ScoringSystem.Instance.RegisterAction(ScoringSystem.ActionType.EnemyBumped);
                    unitsInGame[i].HumanVisualController.ActivateRagdoll();
                    unitsInGame[i].HumanVisualController.ExplosionRagdoll(explosionPosition, force, distance);
                }
                if (unitsInGame[i].AiMovement)
                {
                    unitsInGame[i].AiMovement.Death();
                }
            }
        }
    }

    public void AddBodyPartToQueue(BodyPart part)
    {
        bodyPartsQueueToKill.Add(part);
    }
    IEnumerator BodyPartsKillQueue()
    {
        int j = 0;
        while (true)
        {
            yield return null;
            if (bodyPartsQueueToKill.Count <= 0)
                continue;
            
            for (int i = bodyPartsQueueToKill.Count - 1; i >= 0; i--)
            {
                if (bodyPartsQueueToKill[i] != null)
                    bodyPartsQueueToKill[i].Kill();
                
                bodyPartsQueueToKill.RemoveAt(i);
                
                j++;
                if (j > 3)
                {
                    j = 0;
                    yield return null;
                }
            }
        }
    }
}
