using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using MrPink.Health;
using UnityEngine;

public class UnitVision : MonoBehaviour
{
    private HealthController hc;
    public float fov = 70.0f;
    public float visibilityDistanceMax = 100;
    private RaycastHit hit;
    public LayerMask raycastsLayerMask;
    public Transform raycastOrigin;

    private List<HealthController> visibleEnemies = new List<HealthController>();
    public enum EnemiesSetterBehaviour {SetOnlyOtherTeam, SetAnyone}

    public EnemiesSetterBehaviour setDamagerAsEnemyBehaviour = EnemiesSetterBehaviour.SetOnlyOtherTeam;
    private List<HealthController> enemiesToRemember = new List<HealthController>();

    public List<HealthController> VisibleEnemies
    {
        get { return visibleEnemies; }
    }
    private void Start()
    {
        hc = GetComponent<HealthController>();
        StartCoroutine(CheckEnemies());
    }

    public void SetDamager(HealthController damager)
    {
        if (setDamagerAsEnemyBehaviour == EnemiesSetterBehaviour.SetOnlyOtherTeam && damager.team == hc.team)
            return;
        if (enemiesToRemember.Contains(damager))
            return;
        
        enemiesToRemember.Add(damager);
    }
    
    IEnumerator CheckEnemies()
    {
        while (hc.health > 0)
        {
            for (int i = 0; i < enemiesToRemember.Count; i++)
            {
                var unit = enemiesToRemember[i];
                if (unit != null)
                {
                    CheckUnit(unit, true);
                    yield return new WaitForSeconds(0.1f);
                }
            }
            
            for (int i = 0; i < UnitsManager.Instance.unitsInGame.Count; i++)
            {
                var unit = UnitsManager.Instance.unitsInGame[i];
                if (unit != null && unit.team != hc.team)
                {
                    CheckUnit(unit);
                    yield return new WaitForSeconds(0.1f);
                } 
            }

            yield return null;
        }
        VisibleEnemies.Clear();
    }

    void CheckUnit(HealthController unit, bool ignoreTeams = false)
    {
        if (unit.health <= 0)
        {
            if (VisibleEnemies.Contains(unit))
                VisibleEnemies.Remove(unit);
                    
            return;
        }
                
        if (!ignoreTeams && (unit.team == hc.team || unit.team == Team.NULL || hc.team == Team.NULL))
            return;

        if (LineOfSight(unit.visibilityTrigger.transform))
        {
            if (!VisibleEnemies.Contains(unit))
                VisibleEnemies.Add(unit);
        }
        else if (VisibleEnemies.Contains(unit))
            VisibleEnemies.Remove(unit);
    }

    bool LineOfSight (Transform target) 
    {
        if (Vector3.Angle((target.position + Vector3.one * 1.25f) - raycastOrigin.position, transform.forward) <= fov &&
            Physics.Raycast(raycastOrigin.position, target.position - raycastOrigin.position, out hit, visibilityDistanceMax, raycastsLayerMask) &&
            hit.collider.transform == target)
        {
            return true;
        }

        return false;
    }
}
