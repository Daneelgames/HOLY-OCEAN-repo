using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using Unity.VisualScripting;
using UnityEngine;

namespace MrPink
{
    public class SpyCam : MonoBehaviour
    {
        [Range(0.01f, 5f)]
        public float scanDelay = 0.5f;

        public Team teamToScan;
        public float scanDistance = 15; 
        public LayerMask scanLayerMask;
    
        IEnumerator Start()
        {
            while (true)
            {
                yield return null;
                for (int i = 0; i < UnitsManager.Instance.HcInGame.Count; i++)
                {
                    if (i >= UnitsManager.Instance.HcInGame.Count)
                        continue;
                    var unit = UnitsManager.Instance.HcInGame[i];
                    if (unit == null)
                        continue;
                    if (unit == Game.LocalPlayer.Health)
                        continue;
                    if (unit.health <= 0)
                    {
                        continue;
                    }
                
                    if (PlayerUi.Instance.markedEnemies.ContainsKey(unit))
                        continue;

                    float dist = Vector3.Distance(transform.position, unit.visibilityTrigger.transform.position);
                    if (dist < scanDistance)
                    {
                        if (!Physics.Raycast(transform.position, unit.visibilityTrigger.transform.position - transform.position, dist, scanLayerMask))
                            PlayerUi.Instance.MarkEnemy(unit);
                        else
                            PlayerUi.Instance.UnmarkEnemy(unit);
                    }
                    else
                        PlayerUi.Instance.UnmarkEnemy(unit);
                
                    yield return new WaitForSeconds(scanDelay);
                }
            }
        }
    }
}