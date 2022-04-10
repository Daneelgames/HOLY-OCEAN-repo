using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using UnityEngine;

public class EventOnTimer : MonoBehaviour
{
    public float time = 1;
    public List<ScriptedEvent> TimerEvents;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(time);
        for (int i = 0; i < TimerEvents.Count; i++)
        {
            InteractableEventsManager.Instance.RunEvent(TimerEvents[i], gameObject);   
        }
    }
}
