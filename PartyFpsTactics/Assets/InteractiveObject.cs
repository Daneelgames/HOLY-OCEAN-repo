using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public string interactiveObjectName = "A THING";
    private void Start()
    {
        InteractableManager.Instance.AddInteractable(this);
    }

    private void OnDestroy()
    {
        InteractableManager.Instance.RemoveInteractable(this);
    }
}