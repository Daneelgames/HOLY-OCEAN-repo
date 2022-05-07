using System.Collections;
using System.Collections.Generic;
using _src.Scripts;
using Cysharp.Threading.Tasks.Triggers;
using MrPink;
using MrPink.Health;
using MrPink.Units;
using UnityEngine;

public class UnitsLocalRespawner : MonoBehaviour
{
    public List<HealthController> unitsToRespawnPrefabs;
    public int maxAliveUnits = 10;
    public float respawnCooldown = 5;
    public List<HealthController> spawnedUnits = new List<HealthController>();
    public Transform respawnTransform;
    public float globalCooldownTime = 60;
    public int spawnedBeforeGlobalCooldown = 10;
    int spawnedBeforeGlobalCooldownCurrent = 0;
    public float maxDistanceToPlayerToSpawn = 150;
    void Start()
    {
        StartCoroutine(ManageUnitsRespawn());
    }

    IEnumerator ManageUnitsRespawn()
    {
        while (true)
        {
            for (int i = spawnedUnits.Count - 1; i >= 0; i--)
            {
                var unit = spawnedUnits[i];
                if (unit == null || unit.health <= 0)
                    spawnedUnits.RemoveAt(i);
            }
            
            if (Vector3.Distance(transform.position, Game.Player.Position) < maxDistanceToPlayerToSpawn && spawnedUnits.Count < maxAliveUnits)
            {
                var newUnit = UnitsManager.Instance.SpawnUnit(unitsToRespawnPrefabs[Random.Range(0, unitsToRespawnPrefabs.Count)], respawnTransform.position, respawnTransform);
                spawnedUnits.Add(newUnit);
                spawnedBeforeGlobalCooldownCurrent++;
                if (spawnedBeforeGlobalCooldownCurrent >= spawnedBeforeGlobalCooldown)
                {
                    spawnedBeforeGlobalCooldownCurrent = 0;
                    yield return new WaitForSeconds(globalCooldownTime);
                    continue;
                }
            }
            yield return new WaitForSeconds(respawnCooldown);
        }
    }
}
