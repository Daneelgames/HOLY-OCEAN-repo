using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using UnityEngine;

public class RestrictedZone : MonoBehaviour
{
    public Team ownerTeam = Team.Red;
    public List<Team> allowedTeams = new List<Team>();

    private List<HealthController> hcInside = new List<HealthController>();
    private List<GameObject> unitsInsideGO = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(CheckUnitsInside());
    }

    IEnumerator CheckUnitsInside()
    {
        while (true)
        {
            for (int i = 0; i < hcInside.Count; i++)
            {
                var hc = hcInside[i];
                if (allowedTeams.Contains(hc.team) == false)
                {
                    if (hc.crimeLevel)
                    {
                        hc.crimeLevel.CrimeCommitedAgainstTeam(ownerTeam, false, false);
                    }
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7) // units
        {
            if (!unitsInsideGO.Contains(other.gameObject))
            {
                unitsInsideGO.Add(other.gameObject);
                var hc = other.gameObject.GetComponent<HealthController>();
                if (hc && !hcInside.Contains(hc))
                {
                    hcInside.Add(hc);
                }

            }   
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 7) // units
        {
            if (unitsInsideGO.Contains(other.gameObject))
            {
                var hc = other.gameObject.GetComponent<HealthController>();
                if (hc && hcInside.Contains(hc))
                {
                    hcInside.Remove(hc);
                }

                unitsInsideGO.Remove(other.gameObject);
            }   
        }
    }
    

}
