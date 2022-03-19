using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using Unity.Mathematics;
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

    public void InteractWithIO(InteractiveObject IO)
    {
        for (int i = 0; i < IO.eventsOnInteraction.Count; i++)
        {
            var IOevent = IO.eventsOnInteraction[i];
            
            RunEvent(IOevent);

            if (IOevent.scriptedEventType == ScriptedEventType.DestroyOnInteraction)
            {
                Destroy(IO.gameObject);
            }
        }
    }

    public void RunEvent(ScriptedEvent IOevent, GameObject gameObjectToDestroy = null)
    {
        switch (IOevent.scriptedEventType)
        {
            case ScriptedEventType.SpawnObject:
                GameObject newObj;
                if (IOevent.spawnInsideCamera)
                {
                    newObj = Instantiate(IOevent.prefabToSpawn, Player.Interactor.cam.transform);
                    newObj.transform.localPosition = Vector3.zero;
                    newObj.transform.localRotation = quaternion.identity;
                }
                else
                {
                    newObj = Instantiate(IOevent.prefabToSpawn, Vector3.zero, Quaternion.identity);
                }
                break;
                
                
            case ScriptedEventType.SetCurrentLevel:
                ProgressionManager.Instance.SetCurrentLevel(IOevent.currentLevelToSet);
                break;
                
            case ScriptedEventType.StartProcScene:
                GameManager.Instance.StartProcScene();
                break;
                
            case ScriptedEventType.StartFlatScene:
                GameManager.Instance.StartFlatScene();
                break;
        }
        
        if (gameObjectToDestroy)
            Destroy(gameObjectToDestroy);
    }
    
}


