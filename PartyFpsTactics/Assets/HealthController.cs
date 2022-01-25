using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    public AiMovement AiMovement;

    public enum Team
    {
        Red, Blue
    }

    public Team team;

    public List<BodyPart> bodyParts;

    [ContextMenu("SetHcToParts")]
    public void SetHcToParts()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].hc = this;
        }
    }
}
