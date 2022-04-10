using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using MrPink.Health;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public enum InteractableType
    {
        ItemInteractable, NpcInteractable, VehicleInteractable
    }

    public InteractableType type = InteractableType.ItemInteractable;
    
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
}