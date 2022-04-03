using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateRandomObject : MonoBehaviour
{
    public List<GameObject> objectsToActivate;

    void Start()
    {
        int targetIndex = Random.Range(0, objectsToActivate.Count);

        objectsToActivate[targetIndex].SetActive(true);
        
        for (int i = objectsToActivate.Count - 1; i >= 0; i--)
        {
            if (targetIndex != i)
            {
                Destroy(objectsToActivate[i]);
            }    
        }
    }
}
