using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class PathRequestManager : MonoBehaviour
{
    private Queue<PathResult> results = new Queue<PathResult>();
    
    public static PathRequestManager instance;
    private IslandRoadsGenerator irg;
    private void Start()
    {
        instance = this;
        irg = IslandRoadsGenerator.instance;
    }

    private void Update()
    {
        if (results.Count > 0)
        {
            int itemsInQueue = results.Count;
            lock (results)
            {
                for (int i = 0; i < itemsInQueue; i++)
                {
                    PathResult result = results.Dequeue();
                    result.callback(result.path, result.success);
                }
            }
        }
    }

    public static void RequestPath(PathRequest request, Color[] mapColour)
    {
        ThreadStart threadStart = delegate
        {
            instance.irg.FindPath(request, instance.FinishedProcessingPath, mapColour);
        };
        threadStart.Invoke();
    }

    public void FinishedProcessingPath(PathResult result)
    {
        lock (results)
        {
            results.Enqueue(result);   
        }
    }
}

public struct PathRequest
{
    public Vector2Int pathStart;
    public Vector2Int pathEnd;
    public Action<List<Vector2Int>, bool> callback;

    public PathRequest(Vector2Int start, Vector2Int end, Action<List<Vector2Int>, bool> _callback)
    {
        pathStart = start;
        pathEnd = end;
        callback = _callback;
    }
}

public struct PathResult
{
    public List<Vector2Int> path;
    public bool success;
    public Action<List<Vector2Int>, bool> callback;

    public PathResult(List<Vector2Int> path, bool success, Action<List<Vector2Int>, bool> callback)
    {
        this.path = path;
        this.success = success;
        this.callback = callback;
    }
}