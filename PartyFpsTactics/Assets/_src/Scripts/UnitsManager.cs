using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts;
using MrPink.Health;
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
    public List<BodyPart> bodyPartsQueueToKillCombo = new List<BodyPart>();
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(BodyPartsKillQueue());
    }

    public void RagdollTileExplosion(Vector3 explosionPosition, ScoringActionType action)
    {
        RagdollTileExplosion(explosionPosition, -1, -1, -1, action);
    }
    public void RagdollTileExplosion(Vector3 explosionPosition, float distance = -1, float force = -1, float playerForce = -1, ScoringActionType action = ScoringActionType.NULL)
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
                    
                    unitsInGame[i].Damage(1);
                    if (action != ScoringActionType.NULL)
                        ScoringSystem.Instance.RegisterAction(ScoringActionType.BarrelBumped, 2);
                }

                if (unitsInGame[i].HumanVisualController)
                {
                    if (unitsInGame[i].health > 0 && action != ScoringActionType.NULL)
                        ScoringSystem.Instance.RegisterAction(ScoringActionType.EnemyBumped, 2);
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

    public void AddBodyPartToQueue(BodyPart part, ScoringActionType action)
    {
        if (action != ScoringActionType.NULL)
            bodyPartsQueueToKillCombo.Add(part);
        else
            bodyPartsQueueToKill.Add(part);
    }
    IEnumerator BodyPartsKillQueue()
    {
        int j = 0;
        while (true)
        {
            yield return null;
            
            if (bodyPartsQueueToKillCombo.Count > 0)
            {
                for (int i = bodyPartsQueueToKillCombo.Count - 1; i >= 0; i--)
                {
                    if (bodyPartsQueueToKillCombo[i] != null)
                        bodyPartsQueueToKillCombo[i].Kill(true);

                    bodyPartsQueueToKillCombo.RemoveAt(i);

                    j++;
                    if (j > 3)
                    {
                        j = 0;
                        yield return null;
                    }
                }
            }
            
            if (bodyPartsQueueToKill.Count <= 0)
                continue;

            for (int i = bodyPartsQueueToKill.Count - 1; i >= 0; i--)
            {
                if (bodyPartsQueueToKill[i] != null)
                    bodyPartsQueueToKill[i].Kill(false);

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
