using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableManager : MonoBehaviour
{
    public static InteractableManager Instance;
    public List<InteractiveObject> InteractiveObjects = new List<InteractiveObject>();
    void Awake()
    {
        Instance = this;
    }

    public void AddInteractable(InteractiveObject obj)
    {
        InteractiveObjects.Add(obj);
    }
    public void RemoveInteractable(InteractiveObject obj)
    {
        if (InteractiveObjects.Contains(obj))
            InteractiveObjects.Remove(obj);
    }
}
