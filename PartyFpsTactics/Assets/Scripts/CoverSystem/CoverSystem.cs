using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverSystem : MonoBehaviour
{
    [Range(1,10)]
    public float TakeCoverCooldown = 5;
    [Range(1,20)]
    public float minDistanceToEnemy = 5;
    [Range(1,20)]
    public float minDistanceFromRequester = 5;
    [Range(5,100)]
    public float maxDistanceFromRequester = 30;
    public static CoverSystem Instance;
    public List<Cover> covers = new List<Cover>();
    List<CoverSpot> allCovers = new List<CoverSpot>();
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(InitializeCoverSpots());
    }


    IEnumerator InitializeCoverSpots()
    {
        while (true)
        {
            for (int i = 0; i < covers.Count; i++)
            {
                if (covers[i].Initialized)
                    continue;
                
                for (int j = 0; j < covers[i].coverSpotsList.Count; j++)
                {
                    if (Physics.CheckBox(covers[i].coverSpotsList[j].transform.position + Vector3.up * 0.5f, new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity, 1 << 6))
                    {
                        covers[i].ToggleSpot(j, false);
                    }
                    else if (Physics.CheckBox(covers[i].coverSpotsList[j].transform.position , new Vector3(0.1f, 0.5f, 0.1f), Quaternion.identity, 1 << 6))
                    {
                        covers[i].ToggleSpot(j, true);
                        allCovers.Add(covers[i].coverSpotsList[j]);
                    }
                    yield return null;   
                }

                covers[i].Initialized = true;
            }
            yield return null;
        }
    }

    public List<CoverSpot> GetAllCovers(bool onlyEmpty = true)
    {
        var tempCovers = new List<CoverSpot>(allCovers);
        if (onlyEmpty)
        {
            for (int i = tempCovers.Count - 1; i >= 0; i--)
            {
                if (tempCovers[i].Occupator != null)
                    tempCovers.RemoveAt(i);
            }
        }
        return allCovers;
    }

    public List<CoverSpot> FindCover(Transform requester, List<HealthController> enemiesToHideFrom)
    {
        List<CoverSpot> allGoodSpots = new List<CoverSpot>();
        for (int i = 0; i < enemiesToHideFrom.Count; i++)
        {
            var newSpots = GetAvailableCoverPoints(requester, enemiesToHideFrom[i].transform);
            for (int j = 0; j < newSpots.Count; j++)
            {
                allGoodSpots.Add(newSpots[j]);
            }
        }

        if (allGoodSpots.Count <= 0)
            return allGoodSpots;
        
        for (int i = 0; i < enemiesToHideFrom.Count; i++)
        {
            for (int j = allGoodSpots.Count - 1; j >= 0; j--)
            {
                Vector3 targetDirection = enemiesToHideFrom[i].transform.position - allGoodSpots[j].transform.position;
                Vector3 coverDirection = allGoodSpots[j].parentCover.transform.position - allGoodSpots[j].transform.position;

                if (Vector3.Dot(coverDirection, targetDirection) < 0.5f)
                {
                    allGoodSpots.RemoveAt(j);
                }
            }   
        }

        return allGoodSpots;
    }

    public List<CoverSpot> GetAvailableCoverPoints(Transform requester, Transform transformToCoverFrom)
    {
        List<CoverSpot> coversSpots = new List<CoverSpot>();
        for (int i = 0; i < covers.Count; i++)
        {
            var goodSpots = covers[i].GetGoodCoverSpots(requester, transformToCoverFrom);
            for (int j = 0; j < goodSpots.Count; j++)
            {
                coversSpots.Add(goodSpots[j]);   
            }
        }

        return coversSpots;
    }
    
    
    
    public static bool IsCoveredFrom(HealthController requesterUnit, HealthController targetUnit)
    {
        if (Physics.Raycast(targetUnit.visibilityTrigger.transform.position, (requesterUnit.visibilityTrigger.transform.position - requesterUnit.visibilityTrigger.transform.position).normalized,
            1000, 1 << 6)) // solids layer
        {
            return true;
        }
        
        return false;
    }
}