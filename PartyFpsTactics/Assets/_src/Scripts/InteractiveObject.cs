using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public enum InteractableType
    {
        ItemInteractable, NpcInteractable, VehicleInteractable
    }

    public InteractableType type = InteractableType.ItemInteractable;

    [ShowIf("type", InteractableType.NpcInteractable)]
    public NpcDialoguesList npcDialoguesList;
    
    public string interactiveObjectName = "A THING";
    public List<ScriptedEvent> eventsOnInteraction;
    public Rigidbody rb;
    public HealthController hc;
    private void Start()
    {
        InteractableEventsManager.Instance.AddInteractable(this);
    }

    private void OnDestroy()
    {
        InteractableEventsManager.Instance.RemoveInteractable(this);
    }

    public void PlayerInteraction()
    {
        CheckNpcDialogueList();
        InteractableEventsManager.Instance.InteractWithIO(this);
    }

    public void CheckNpcDialogueList()
    {
        if (type == InteractableType.NpcInteractable && npcDialoguesList)
        {
            npcDialoguesList.TryToReplaceEventsBasedOnConditions(this);
        }   
    }

    public void SetNewEvents(List<ScriptedEvent> newEvents)
    {
        eventsOnInteraction = new List<ScriptedEvent>(newEvents);
    }
}