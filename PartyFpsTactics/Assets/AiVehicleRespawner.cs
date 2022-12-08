using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;
using Random = UnityEngine.Random;

public class AiVehicleRespawner : MonoBehaviour
{
    [SerializeField] private Vector2 playerDistanceToRespawnMinMax = new Vector2(30, 150);
    [SerializeField] private List<AiVehicleControls> vehiclePrefabs;
    [SerializeField] private List<Transform> roadSpawnPositions = new List<Transform>();
    [SerializeField] private float respawnCooldown = 1;
    [SerializeField] private int maxAliveAiVehicles = 3;

    private List<AiVehicleControls> spawnedVehicles = new List<AiVehicleControls>();

    private void OnEnable()
    {
        StartCoroutine(RespawningVehicles());
    }

    IEnumerator RespawningVehicles()
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnCooldown);
            
            if (spawnedVehicles.Count >= maxAliveAiVehicles)
                continue;

            if (spawnedVehicles.Count > 0)
            {
                for (var index = spawnedVehicles.Count - 1; index >= 0; index--)
                {
                    var spawnedVehicle = spawnedVehicles[index];
                    if (spawnedVehicle == null)
                    {
                        spawnedVehicles.RemoveAt(index);
                        continue;
                    }
                    if (spawnedVehicle.CarHc.IsDead)
                        spawnedVehicles.Remove(spawnedVehicle);
                }
            }

            List<Transform> spawnersTemp = new List<Transform>();
            foreach (var roadSpawnPosition in roadSpawnPositions)
            {
                var distance = Vector3.Distance(roadSpawnPosition.position, Game.Player.Position);
                if (distance > playerDistanceToRespawnMinMax.y || distance < playerDistanceToRespawnMinMax.x)
                    continue;
                
                spawnersTemp.Add(roadSpawnPosition);
            }

            if (spawnersTemp.Count < 1)
            {
                Debug.LogError("CANT SPAWN AI CAR. NO SPAWNER FOUND.");
                continue;
            }
            Vector3 spawnPos = spawnersTemp[Random.Range(0, spawnersTemp.Count)].position;
            var newVeh = Instantiate(vehiclePrefabs[Random.Range(0, vehiclePrefabs.Count)], spawnPos, Quaternion.LookRotation(Game.Player.Position - spawnPos, Vector3.up));
            spawnedVehicles.Add(newVeh);
        }
    }
}
