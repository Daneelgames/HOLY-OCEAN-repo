using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelEventActor : MonoBehaviour
{
    public int actorId = 0;

    private void Start()
    {
        LevelEventsOnConditions.Instance.AddActor(this);
    }
}
