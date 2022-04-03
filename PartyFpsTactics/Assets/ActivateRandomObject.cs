using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ActivateRandomObject : MonoBehaviour
{
    public List<ListOfGameObjects> objectsToActivate;

    void Start()
    {
        int targetIndex = Random.Range(0, objectsToActivate.Count);

        for (int j = 0; j < objectsToActivate[targetIndex].GameObjects.Count; j++)
        {
            objectsToActivate[targetIndex].GameObjects[j].SetActive(true);
        }
        
        for (int i = objectsToActivate.Count - 1; i >= 0; i--)
        {
            if (targetIndex != i)
            {
                
                for (int j = 0; j < objectsToActivate[i].GameObjects.Count; j++)
                {
                    Destroy(objectsToActivate[i].GameObjects[j]);
                }
            }    
        }
    }
}

[Serializable]
public class ListOfGameObjects
{
    public List<GameObject> GameObjects;
}