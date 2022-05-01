using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class IslandWalker : MonoBehaviour
{
    public Transform target;
    public float updatePathTime = 1;
    public float stepTime = 10;
    private DynamicLevelGenerator dlg;
    private IslandRoadsGenerator irg;
    private List<Vector2Int> path = new List<Vector2Int>();
    private int targetPathIndex;
    
    void Start()
    {
        dlg = DynamicLevelGenerator.instance;
        irg = IslandRoadsGenerator.instance;

        target = dlg.playerTarget.transform;
        
        StartCoroutine(RequestPath());
    }

    IEnumerator RequestPath()
    {
        while (true)
        {
            yield return new WaitForSeconds(updatePathTime);
            RequestPathOnce();
        }
    }

    void RequestPathOnce()
    {
        int distance = 10000;
        int newDistance = 0;
        Vector2Int targetTilePos;
        Vector2Int currentCoords;

        // request path to closest neighbour of the target
        distance = 10000;
        var targetsNeighbourNodes = dlg.GetCoordsOfClosestNeighbourOfTarget(target.position);
        if (targetsNeighbourNodes.Count <= 0)
            return;
            
        currentCoords = dlg.GetCoordinatesFromWorldPosition(transform.position);
        targetTilePos = Vector2Int.zero;
        for (int i = 0; i < targetsNeighbourNodes.Count; i++)
        {
            newDistance = IslandRoadsGenerator.instance.GetDistance(currentCoords, new Vector2Int(targetsNeighbourNodes[i].gridX, targetsNeighbourNodes[i].gridY));
            if (newDistance <= distance)
            {
                distance = newDistance;
                targetTilePos = new Vector2Int(targetsNeighbourNodes[i].gridX, targetsNeighbourNodes[i].gridY);
            }
        }
        PathRequestManager.RequestPath(new PathRequest(currentCoords, targetTilePos, OnPathFound), null);   
    }
    
    public void OnPathFound(List<Vector2Int> newPath, bool pathFound)
    {
        if (!pathFound)
            return;
        
        path = newPath;
        StopCoroutine(FollowPath());
        StartCoroutine(FollowPath());
    }

    IEnumerator FollowPath()
    {
        targetPathIndex = 0;
        float t = 0;
        float tt = 1;
        Vector3 stepStartPosition;
        Vector3 stepEndPosition;
        
        while (true)
        {
            if (targetPathIndex >= path.Count)
            {
                yield break;
            }
            
            if (dlg.spawnedTiles[path[targetPathIndex].x, path[targetPathIndex].y] == null || dlg.spawnedTiles[path[targetPathIndex].x, path[targetPathIndex].y].spawnedTile == null)
            {
                yield return null;
                continue;
            }
            
            // get info about the next step
            stepStartPosition = transform.position;
            stepEndPosition = dlg.spawnedTiles[path[targetPathIndex].x, path[targetPathIndex].y].spawnedTile.transform.position;
            t = 0;
            tt = stepTime;
            
            while (t < tt)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(stepStartPosition, stepEndPosition, t/tt);
                transform.LookAt(stepEndPosition);
                yield return null;
            }
            targetPathIndex++;
            if (targetPathIndex >= path.Count)
            {
                yield break;
            }
            yield return null;
        }
    }
}