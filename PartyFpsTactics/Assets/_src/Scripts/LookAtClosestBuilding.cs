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
        yield return null;
    }

}
