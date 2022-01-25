using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    public AiMovement AiMovement;
    public Collider visibilityTrigger;
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

    [ContextMenu("SetHcToParts")]
    public void SetHcToParts()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].hc = this;
        }
    }
}
