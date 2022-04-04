using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Cover : MonoBehaviour
{
    public List<CoverSpot> coverSpotsActive = new List<CoverSpot>();
    public List<CoverSpot> coverSpotsList = new List<CoverSpot>();
    public bool Initialized = false;
    
    [Header("Test")]
    public Transform testTarget;
    private void Start()
    {
        CoverSystem.Instance.covers.Add(this);
    }

    public void ConstructSpots()
    {
        GameObject spotsParent = new GameObject("Spots");
        spotsParent.transform.parent = transform;
        spotsParent.transform.localPosition = Vector3.zero;
        spotsParent.transform.localRotation = Quaternion.identity;
        
        for (int i = 0; i < 4; i++)
        {
            GameObject newSpot = new GameObject(i.ToString());
            newSpot.transform.parent = spotsParent.transform.parent;
            switch (i)
            {
                case 0:
                    newSpot.transform.localPosition = new Vector3(0,0,-0.75f);       
                    newSpot.transform.localRotation = Quaternion.Euler(0,180,0);       
                    break;
                case 1:
                    newSpot.transform.localPosition = new Vector3(0,0,0.75f);       
                    newSpot.transform.localRotation = Quaternion.Euler(0,0,0);       
                    break;
                case 2:
                    newSpot.transform.localPosition = new Vector3(-0.75f,0,0);       
                    newSpot.transform.localRotation = Quaternion.Euler(0,270,0);       
                    break;
                case 3:
                    newSpot.transform.localPosition = new Vector3(0.75f,0,0);       
                    newSpot.transform.localRotation = Quaternion.Euler(0,90,0);       
                    break;
            }

            var spotComponent = newSpot.AddComponent<CoverSpot>();
            spotComponent.parentCover = transform;
            coverSpotsList.Add(spotComponent);
        }
    }

    private void OnDestroy()
    {
        if (CoverSystem.Instance.covers.Contains(this))
            CoverSystem.Instance.covers.Remove(this);
    }

    /*
    private void OnDrawGizmos()
    {
        for (int i = 0; i < coverSpotsActive.Count; i++)
        {
            if (coverSpotsActive[i] == null)
                continue;
            
            Gizmos.DrawWireCube(coverSpotsActive[i].transform.position + Vector3.up * 0.25f, Vector3.one / 2);
        }
    }*/

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