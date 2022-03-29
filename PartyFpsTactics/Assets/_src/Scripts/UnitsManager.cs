using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts;
using MrPink;
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

    private List<BasicHealth> _bodyPartsQueueToKill = new List<BasicHealth>();
    private List<BasicHealth> _bodyPartsQueueToKillCombo = new List<BasicHealth>();
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
                    
                    unitsInGame[i].Damage(1, DamageSource.Player);
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

    public void AddHealthEntityToQueue(BasicHealth part, ScoringActionType action)
    {
        if (action != ScoringActionType.NULL)
            _bodyPartsQueueToKillCombo.Add(part);
        else
            _bodyPartsQueueToKill.Add(part);
    }
    
    private IEnumerator BodyPartsKillQueue()
    {
        int j = 0;
        while (true)
        {
            yield return null;
            
            if (_bodyPartsQueueToKillCombo.Count > 0)
            {
                for (int i = _bodyPartsQueueToKillCombo.Count - 1; i >= 0; i--)
                {
                    if (_bodyPartsQueueToKillCombo[i] != null)
                        _bodyPartsQueueToKillCombo[i].Kill(DamageSource.Player);

                    _bodyPartsQueueToKillCombo.RemoveAt(i);

                    j++;
                    if (j > 3)
                    {
                        j = 0;
                        yield return null;
                    }
                }
            }
            
            if (_bodyPartsQueueToKill.Count <= 0)
                continue;

            for (int i = _bodyPartsQueueToKill.Count - 1; i >= 0; i--)
            {
                if (_bodyPartsQueueToKill[i] != null)
                    _bodyPartsQueueToKill[i].Kill(DamageSource.Environment);

                _bodyPartsQueueToKill.RemoveAt(i);
                
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
