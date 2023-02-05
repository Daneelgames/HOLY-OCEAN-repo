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
        var buildings = BuildingGenerator.Instances;
        float distance = 10000;
        
        while (true)
        {
            yield return null;
            
            if (buildings.Count <= 0)
                continue;
            
            for (int i = 0; i < buildings.Count; i++)
            {
                yield return null;
                
                closestBuildingPosition = buildings[i].spawnedBuildings[0].worldPos;
            }
        }
    }

    private void Update()
    {
        transform.LookAt(closestBuildingPosition);
    }
}
