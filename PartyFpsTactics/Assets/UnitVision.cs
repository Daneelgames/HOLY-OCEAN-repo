using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class UnitVision : MonoBehaviour
{
    private HealthController hc;
    float fov = 70.0f;
    private RaycastHit hit;
    public LayerMask raycastsLayerMask;
    public Transform raycastOrigin;

    private void Start()
    {
        hc = GetComponent<HealthController>();
        StartCoroutine(CheckEnemies());
    }

    IEnumerator CheckEnemies()
    {
        while (true)
        {
            for (int i = 0; i < GameManager.Instance.ActiveHealthControllers.Count; i++)
            {
                if (GameManager.Instance.ActiveHealthControllers[i].team == hc.team)
                    continue;

                if (LineOfSight(GameManager.Instance.ActiveHealthControllers[i].visibilityTrigger.transform))
                {
                    Debug.Log(gameObject.name + " sees " + GameManager.Instance.ActiveHealthControllers[i].name);
                }
                yield return null;   
            }
            
            yield return null;
        }
    }

    bool LineOfSight (Transform target) 
    {
        if (Vector3.Angle((target.position + Vector3.one * 1.25f) - raycastOrigin.position, transform.forward) <= fov &&
            Physics.Linecast(raycastOrigin.position, target.position, out hit, raycastsLayerMask) &&
            hit.collider.transform == target)
        {
            return true;
        }

        return false;
    }
}
