using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using MrPink;
using UnityEngine;

public class EventOnPlayerEntersTrigger : MonoBehaviour
{
    public List<ScriptedEvent> eventsToRunOnTriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Game.LocalPlayer.Health.gameObject)
        {
            for (int i = 0; i < eventsToRunOnTriggerEnter.Count; i++)
            {
                InteractableEventsManager.Instance.RunEvent(eventsToRunOnTriggerEnter[i], null, gameObject);   
            }
        }
    }
}
