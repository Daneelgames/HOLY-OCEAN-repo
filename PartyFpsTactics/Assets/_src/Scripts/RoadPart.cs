using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPart : MonoBehaviour
{
    public List<Transform> raycastTransforms;
    public Transform roadStart;
    public List<Transform> roadEnds;
    public GameObject visualGo;

    public List<BoxCollider> collidersToCheck;

    public void Init()
    {
        visualGo.SetActive(false);
        for (int i = 0; i < collidersToCheck.Count; i++)
        {
            if (Physics.CheckBox(collidersToCheck[i].transform.position,
                collidersToCheck[i].size, collidersToCheck[i].transform.rotation, GameManager.Instance.AllSolidsMask))
            {
                // SPAWN A WAY TO GET TO THE END AND TO THE START OF PART FROM THE GROUND / CLIFF
                Destroy(gameObject);
                return;
            }
        }

        for (int i = collidersToCheck.Count - 1; i >= 0; i--)
        {
          Destroy(collidersToCheck[i].gameObject);  
        }
        
        collidersToCheck.Clear();
        visualGo.SetActive(true);
    }

    private void OnDestroy()
    {
        RoadGenerator.Instance.RemoveFromSpawnedParts(this);
    }
}
