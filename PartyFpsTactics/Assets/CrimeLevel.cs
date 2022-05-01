using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using UnityEngine;

public class CrimeLevel : MonoBehaviour
{
    private HealthController ownHc;

    private void Start()
    {
        ownHc = gameObject.GetComponent<HealthController>();
    }

    public void CrimeCommitedAgainstTeam(Team teamAgainst, bool stack, bool tellToFriends)
    {
        Debug.Log("CrimeCommitedAgainstTeam; teamAgains = " + teamAgainst + "; ownHc = " + ownHc);
        var visibleByUnits = ownHc.unitsVisibleBy;
        for (int i = 0; i < visibleByUnits.Count; i++)
        {
            var unit = visibleByUnits[i];
            if (!unit)
                continue;
            if (unit.team == teamAgainst)
                unit.UnitVision.SetDamager(ownHc, stack, tellToFriends);
        }
    }
}