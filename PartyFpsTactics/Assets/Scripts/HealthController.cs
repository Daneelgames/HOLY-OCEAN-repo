using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    public int health = 100;
    
    public Collider visibilityTrigger;
    
    [Header("AI")]
    public AiMovement AiMovement;
    public AiWeaponControls AiWeaponControls;
    public HumanVisualController HumanVisualController;

    public enum Team
    {
        Red, Blue
    }

    public Team team;

    public List<BodyPart> bodyParts;

    private void Start()
    {
        GameManager.Instance.ActiveHealthControllers.Add(this);
    }

    [ContextMenu("GetBodyParts")]
    public void GetBodyParts()
    {
        bodyParts.Clear();
        var parts = transform.GetComponentsInChildren<BodyPart>();
        for (int i = 0; i < parts.Length; i++)
        {
            bodyParts.Add(parts[i]);
            parts[i].hc = this;
        }
    }
    [ContextMenu("SetHcToParts")]
    public void SetHcToParts()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].hc = this;
        }
    }

    public void Damage(int damage)
    {
        health -= damage;

        if (health <= 0)
            Death();
    }

    void Death()
    {
        if (AiMovement)
            AiMovement.Death();

        HumanVisualController.DeathRagdoll();
    }

    void OnDestroy()
    {
        if (GameManager.Instance.ActiveHealthControllers.Contains(this))
            GameManager.Instance.ActiveHealthControllers.Remove(this);
        if (CommanderControls.Instance.unitsInParty.Contains(this))
            CommanderControls.Instance.unitsInParty.Remove(this);
    }
}
