using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class PathWalker : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    [SerializeField] private float moveSpeed = 30;
    [SerializeField] private Transform currentClosestPoint;
    [SerializeField] private Transform nextTargetPoint;
    private void Start()
    {
        StartCoroutine(FollowPath());
    }

    void GetTarget()
    {
        float distance = 1000;
        Transform closest = null;
        foreach (var waypoint in waypoints)
        {
            var newDist = Vector3.Distance(waypoint.position, transform.position);
            if (newDist > distance)
                continue;
            distance = newDist;
            closest = waypoint;
        }

        currentClosestPoint = closest;
        var indexInList = waypoints.IndexOf(closest);
        if (indexInList >= waypoints.Count - 1)
            nextTargetPoint = waypoints[0];
        else
            nextTargetPoint = waypoints[indexInList + 1];

        // get closestPoint
        // select next point from closest in list
    }
    IEnumerator FollowPath()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            GetTarget();
            _navMeshAgent.speed = moveSpeed;
            _navMeshAgent.SetDestination(nextTargetPoint.position);
        }
    }

    [Button]
    public void SortPathPoints()
    {
        List<Transform> sortedTemp = new List<Transform>();
        
        sortedTemp.Add(waypoints[0]);    
        while (sortedTemp.Count < waypoints.Count)
        {
            var lastSorted = sortedTemp[sortedTemp.Count - 1];
            float distance = 1000;
            Transform closestToSorted = null;
            foreach (var waypoint in waypoints)
            {
                if (sortedTemp.Contains(waypoint)) continue;
                
                var newDist = Vector3.Distance(waypoint.position, lastSorted.position);
                if (newDist > distance)
                    continue;
                distance = newDist;
                closestToSorted = waypoint;
            }

            if (closestToSorted == null)
            {
                Debug.LogError("CANT FIND CLOSEST TO SORTED");
                break;
            }
            sortedTemp.Add(closestToSorted);
        }

        waypoints = new List<Transform>(sortedTemp);
    }
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            var first = waypoints[i].position;
            var second = waypoints[0].position;
            if (i < waypoints.Count - 1)
                second = waypoints[i + 1].position;
            Gizmos.DrawLine(first, second);
        }
    }
}
