using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Cover : MonoBehaviour
{
    public List<CoverSpot> coverSpotsActive;
    public List<CoverSpot> coverSpotsList;
    public bool Initialized = false;
    
    [Header("Test")]
    public Transform testTarget;
    private void Start()
    {
        CoverSystem.Instance.covers.Add(this);
    }

    private void OnDestroy()
    {
        if (CoverSystem.Instance.covers.Contains(this))
            CoverSystem.Instance.covers.Remove(this);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < coverSpotsActive.Count; i++)
        {
            if (coverSpotsActive[i] == null)
                continue;
            
            Gizmos.DrawWireCube(coverSpotsActive[i].transform.position + Vector3.up * 0.25f, Vector3.one / 2);
        }
    }

    [ContextMenu("TestGoodSpotsAgainstTarget")]
    public void TestGoodSpotsAgainstTarget()
    {
        ToggleSpot(0, true);
        ToggleSpot(1, true);
        ToggleSpot(2, true);
        ToggleSpot(3, true);
        var newPoints = GetGoodCoverSpots(transform, testTarget);
        ToggleSpot(0, false);
        ToggleSpot(1, false);
        ToggleSpot(2, false);
        ToggleSpot(3, false);
        for (int i = 0; i < newPoints.Count; i++)
        {
            ToggleSpot(coverSpotsList.IndexOf(newPoints[i]), true);
        }
    }
    
    public List<CoverSpot> GetGoodCoverSpots(Transform requester, Transform targetToCoverFrom)
    {
        List<CoverSpot> goodCovers = new List<CoverSpot>();
        for (int i = 0; i < coverSpotsActive.Count; i++)
        {
            if (coverSpotsActive[i].Occupator != null)
                continue;
            
            if (Application.isPlaying)
            {
                if (Vector3.Distance(targetToCoverFrom.position, coverSpotsActive[i].transform.position) <
                    CoverSystem.Instance.minDistanceToEnemy)
                    continue;

                if (Vector3.Distance(requester.position, coverSpotsActive[i].transform.position) >
                    CoverSystem.Instance.maxDistanceFromRequester)
                    continue;
                if (Vector3.Distance(requester.position, coverSpotsActive[i].transform.position) <
                    CoverSystem.Instance.minDistanceFromRequester)
                    continue;
            }
            else
            {
                if (Vector3.Distance(targetToCoverFrom.position, coverSpotsActive[i].transform.position) < 3)
                    continue;

                if (Vector3.Distance(requester.position, coverSpotsActive[i].transform.position) > 30)
                    continue;
                if (Vector3.Distance(requester.position, coverSpotsActive[i].transform.position) < 5)
                    continue;
            }
            
            Vector3 targetDirection = targetToCoverFrom.position - coverSpotsActive[i].transform.position;
            Vector3 coverDirection = transform.position - coverSpotsActive[i].transform.position;

            if (Vector3.Dot(coverDirection, targetDirection) > 0.5f)
            {
                goodCovers.Add(coverSpotsActive[i]);
            }
        }

        return goodCovers;
    }

    
    public void ToggleSpot(int index, bool add)
    {
        if (add && coverSpotsActive.Contains(coverSpotsList[index]) == false)
            coverSpotsActive.Add(coverSpotsList[index]);
        else if (!add && coverSpotsActive.Contains(coverSpotsList[index]))
            coverSpotsActive.Remove(coverSpotsList[index]);
        
    }
}