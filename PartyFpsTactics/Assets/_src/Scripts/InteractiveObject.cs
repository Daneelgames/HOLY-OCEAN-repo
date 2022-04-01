using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public enum InteractableType
    {
        ItemInteractable, NpcInteractable
    }

    public InteractableType type = InteractableType.ItemInteractable;
    
    public string interactiveObjectName = "A THING";
    public List<ScriptedEvent> eventsOnInteraction;
    public Rigidbody rb;
    private void Start()
    {
        InteractableManager.Instance.AddInteractable(this);
    }

    private void OnDestroy()
    {
        InteractableManager.Instance.RemoveInteractable(this);
    }
}