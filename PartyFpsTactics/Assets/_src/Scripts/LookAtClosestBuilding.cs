using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtClosestBuilding : MonoBehaviour
{
    private Vector3 closestBuildingPosition;

    private void Start()
    {
        StartCoroutine(GetClosestBuilding());
    }

    IEnumerator GetClosestBuilding()
    {
        var buildings = BuildingGenerator.Instance.spawnedBuildings;
        float distance = 10000;
        
        while (true)
        {
            yield return null;
            
            if (BuildingGenerator.Instance.spawnedBuildings.Count <= 0)
                continue;
            
            for (int i = 0; i < buildings.Count; i++)
            {
                yield return null;
                
                closestBuildingPosition = buildings[i].worldPos;
                continue;
                
                float newDist = Vector3.Distance(transform.position, buildings[i].worldPos);
                if (newDist < distance)
                {
                    distance = newDist;
                    closestBuildingPosition = buildings[i].worldPos;
                }
            }
        }
    }

    private void Update()
    {
        transform.LookAt(closestBuildingPosition);
    }
}
