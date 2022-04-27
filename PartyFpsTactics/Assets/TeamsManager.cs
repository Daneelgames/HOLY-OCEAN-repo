using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
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
}

[Serializable]
public class TeamsRelationships
{
    public Team team;
    public List<Team> enemyTeams;
}