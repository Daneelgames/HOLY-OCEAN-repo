using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using Unity.AI.Navigation;
using UnityEngine;

public class RoadPart : MonoBehaviour
{
    public enum RoadType
    {
        Straight,
        StraightVertical,
        Turn90,
        Turn90Vertical
    }

    public RoadType roadType = RoadType.Straight;
    
    public List<Transform> raycastTransforms;
    public Transform roadStart;
    public List<Transform> roadEnds;
    public GameObject visualGo;

    public List<BoxCollider> collidersToCheck;

    public NavMeshSurface navMeshSurface;

    public void Init()
    {
        visualGo.SetActive(false);
        for (int i = 0; i < collidersToCheck.Count; i++)
        {
            var hits = Physics.OverlapBox(collidersToCheck[i].transform.position, collidersToCheck[i].size, collidersToCheck[i].transform.rotation, GameManager.Instance.AllSolidsMask);
            
            for (int j = hits.Length - 1; j >= 0; j--)
            {
                var tile = hits[j].gameObject.GetComponent<TileHealth>();
                if (tile)
                {
                    tile.Kill(DamageSource.Environment);
                    continue;
                }
                
                if (RoadGenerator.Instance.spawnedRoadPartsGO.Contains(hits[j].gameObject))
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        for (int i = collidersToCheck.Count - 1; i >= 0; i--)
        {
            Destroy(collidersToCheck[i].gameObject);  
        }
        
        collidersToCheck.Clear();
        visualGo.SetActive(true);
        
        navMeshSurface.BuildNavMesh();
    }

    private void OnDestroy()
    {
        RoadGenerator.Instance.RemoveFromSpawnedParts(this);
    }
}
