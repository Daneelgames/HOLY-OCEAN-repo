using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using UnityEngine;

public class NpcDialoguesList : MonoBehaviour
{
    [Header("REPLACE INTERACTION EVENTS IF CONDITION MET")]
    [Header("EVENTS WILL FIRE ON PLAYERS INTERACTION")]
    public List<LevelEvent> eventsOnConditions;
    
    public void TryToReplaceEventsBasedOnConditions(InteractiveObject interactiveObject)
    {
        List<ScriptedEvent> newScriptedEvents = new List<ScriptedEvent>();
        for (int i = 0; i < eventsOnConditions.Count; i++)
        {
            var _event = eventsOnConditions[i];
            if (LevelEventsOnConditions.Instance.CheckEventOnce(_event))
            {
                // all conditions met

                newScriptedEvents = new List<ScriptedEvent>(_event.events);
                eventsOnConditions.RemoveAt(i);
                break;
            }
        }
        
        if (newScriptedEvents.Count > 0)
            interactiveObject.SetNewEvents(newScriptedEvents);
    }
}
