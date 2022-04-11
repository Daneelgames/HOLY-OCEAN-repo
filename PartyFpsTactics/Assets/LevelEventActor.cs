using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEventActor : MonoBehaviour
{
    public int actorId = 0;
    public InteractiveObject npcInteraction;

    private void Start()
    {
        LevelEventsOnConditions.Instance.AddActor(this);
    }
}
