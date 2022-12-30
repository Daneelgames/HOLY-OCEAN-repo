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
}
