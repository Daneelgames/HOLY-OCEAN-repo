using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using UnityEngine;

public class TeamsManager : MonoBehaviour
{
    public static TeamsManager Instance;

    public List<TeamsRelationships> TeamsRelationshipsList;
    private void Awake()
    {
        Instance = this;
    }

    public bool IsUnitEnemyToMe(Team myTeam, Team unitTeam)
    {
        for (int i = 0; i < TeamsRelationshipsList.Count; i++)
        {
            if (TeamsRelationshipsList[i].team == myTeam)
            {
                if (TeamsRelationshipsList[i].enemyTeams.Contains(unitTeam))
                    return true;
                
                break;
            }
        }
        return false;
    }

    public HealthController FindClosestEnemyInRange(Team myTeam, Vector3 myPos, float range = 1000)
    {
        HealthController closestEnemy = null;
        float distance = range;
        for (int i = 0; i < UnitsManager.Instance.HcInGame.Count; i++)
        {
            var unit = UnitsManager.Instance.HcInGame[i];
            if (unit == null || unit.health <= 0)
                continue;

            if (IsUnitEnemyToMe(myTeam, unit.team))
            {
                float newDist = Vector3.Distance(myPos, unit.transform.position);
                if (newDist < distance)
                {
                    distance = newDist;
                    closestEnemy = unit;
                }
            }
        }
        return closestEnemy;
    }
}

[Serializable]
public class TeamsRelationships
{
    public Team team;
    public List<Team> enemyTeams;
}