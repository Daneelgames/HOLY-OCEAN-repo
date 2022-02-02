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
    public float tileExplosionForcePlayer = 100;
    private void Awake()
    {
        Instance = this;
    }

    public void RagdollTileExplosion(Vector3 explosionPosition)
    {
        for (int i = 0; i < unitsInGame.Count; i++)
        {
            if (Vector3.Distance(explosionPosition, unitsInGame[i].transform.position + Vector3.up) <= tileExplosionDistance)
            {
                Debug.Log("Explode Rigidbody! " + unitsInGame[i].gameObject.name);
                if (unitsInGame[i].playerMovement)
                {
                    unitsInGame[i].playerMovement.rb
                        .AddForce((unitsInGame[i].visibilityTrigger.transform.position - explosionPosition).normalized * tileExplosionForcePlayer, ForceMode.VelocityChange);
                    continue;
                }

                if (unitsInGame[i].HumanVisualController)
                {
                    unitsInGame[i].HumanVisualController.DeathRagdoll();
                    unitsInGame[i].HumanVisualController.ExplosionRagdoll(explosionPosition, tileExplosionForce, tileExplosionDistance);
                }
                if (unitsInGame[i].AiMovement)
                {
                    unitsInGame[i].AiMovement.Death();
                }
            }
        }
    }
}
