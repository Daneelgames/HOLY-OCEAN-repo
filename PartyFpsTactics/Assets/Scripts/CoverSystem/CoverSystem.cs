using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverSystem : MonoBehaviour 
{
    public static CoverSystem Instance;
    public List<Cover> covers = new List<Cover>();
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
                for (int j = 0; j < covers[i].coverSpotsList.Count; j++)
                {
                    if (Physics.CheckBox(covers[i].coverSpotsList[j].position + Vector3.up * 0.5f, new Vector3(0.3f, 0.3f, 0.3f), Quaternion.identity, 1 << 6))
                    {
                        covers[i].ToggleSpot(j, false);
                    }
                    else if (Physics.CheckBox(covers[i].coverSpotsList[j].position , new Vector3(0.3f, 1, 0.3f), Quaternion.identity, 1 << 6))
                    {
                        covers[i].ToggleSpot(j, true);
                    }
                    return null;   
                }
            }
            return null;
        }
    }

    public List<Transform> GetAvailableCoverPoints(Transform transformToCoverFrom)
    {
        List<Transform> coversSpots = new List<Transform>();
        for (int i = 0; i < covers.Count; i++)
        {
            var goodSpots = covers[i].GetGoodCoverSpots(transformToCoverFrom);
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