using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertGenerator : MonoBehaviour
{
    public List<GameObject> cubeRockPrefabs;
    public Vector3 zonePosOffset = Vector3.zero;
    public Vector3 zoneSize = Vector3.one;
    
    [ContextMenu("GenerateDesert")]
    public void GenerateDesert()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawWireCube(transform.position + Vector3.up * zoneSize.y / 2 + zonePosOffset, zoneSize);
    }
}
