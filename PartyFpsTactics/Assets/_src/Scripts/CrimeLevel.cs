using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.Units;
using UnityEngine;

public class CrimeLevel : MonoBehaviour
{
    private HealthController ownHc;

    private void Start()
    {
        ownHc = gameObject.GetComponent<HealthController>();
    }

    public void CrimeCommitedAgainstTeam(Team teamAgainst, bool stack, bool tellToFriends, bool setFollowIntruder = false)
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

        if (setFollowIntruder)
        {
            foreach (var healthController in UnitsManager.Instance.MobsInGame)
            {
                if (healthController == null || healthController.health <= 0)
                    continue;
                
                if (healthController.team == teamAgainst)
                {
                    // ReSharper disable once Unity.NoNullPropagation
                    healthController.AiMovement?.FollowIntruder(ownHc);
                }
            }
        }
    }
    
    
}