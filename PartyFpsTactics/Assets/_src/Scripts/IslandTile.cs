using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.Units;
using UnityEngine;
using Random = UnityEngine.Random;

public class IslandTile : MonoBehaviour
{
    public Pooling.IslandTilePool IslandTilePool = new Pooling.IslandTilePool();
    public List<Vector3> islandTileStaticSpawnersLocalPositions = new List<Vector3>();

    public List<InteractiveObject> spawnedLoot = new List<InteractiveObject>();
    public List<HealthController> spawnedUnits = new List<HealthController>();
    public List<GameObject> spawnedProps = new List<GameObject>();
    [ContextMenu("ClearSpawners")]
    void ClearSpawners()
    {
        islandTileStaticSpawnersLocalPositions.Clear();   
    }
    [ContextMenu("RemoveDoubles")]
    void RemoveDoubles()
    {
        float distance = 3;
        for (int i = islandTileStaticSpawnersLocalPositions.Count - 1; i >= 0; i--)
        {
            
            for (int j = islandTileStaticSpawnersLocalPositions.Count - 1; j >= 0; j--)
            {
                if (islandTileStaticSpawnersLocalPositions.Count <= i)
                    continue;
                if (islandTileStaticSpawnersLocalPositions.Count <= j)
                    continue;
                
                if (i == j)
                    continue;
                
                var pos0 = islandTileStaticSpawnersLocalPositions[i];
                var pos1 = islandTileStaticSpawnersLocalPositions[j];

                if (Vector3.Distance(pos0, pos1) <= distance)
                    islandTileStaticSpawnersLocalPositions.RemoveAt(j);
            }
        }
    }
    [ContextMenu("GenerateSpawners")]
    void GenerateSpawners()
    {
        for (int i = 0; i < 500; i++)
        {
            Vector3 originPos = transform.position + Vector3.up * Random.Range(-500, 500) + new Vector3(Random.Range(-200, 200), 0, Random.Range(-200, 200));
            if (Physics.Raycast(originPos, (transform.position - Vector3.up * Random.Range(0, 500)) - originPos, out var hit, 10000, 1 << 6))
            {
                if (Vector3.Angle(hit.normal, Vector3.up) < 60)
                    islandTileStaticSpawnersLocalPositions.Add(transform.InverseTransformPoint(hit.point));
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < islandTileStaticSpawnersLocalPositions.Count; i++)
        {
            Gizmos.DrawCube(transform.TransformPoint(islandTileStaticSpawnersLocalPositions[i]), Vector3.one);
        }
    }
}