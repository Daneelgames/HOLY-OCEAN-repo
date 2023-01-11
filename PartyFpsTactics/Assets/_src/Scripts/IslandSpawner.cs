using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MrPink;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

public class IslandSpawner : NetworkBehaviour
{
    public static IslandSpawner Instance;    
    [SerializeField] [ReadOnly] private List<Island> spawnedIslands = new List<Island>();
    

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        Instance = this;
        StartCoroutine(CullIslandsLocally());
    }

    public float GetDistanceToClosestIsland(Vector3 posAsking)
    {
        float distance = 100000f;
        foreach (var island in spawnedIslands)
        {
            var pos = island.gameObject.transform.position;
            var newDistance = Vector3.Distance(pos, posAsking);
            if (newDistance < distance)
                distance = newDistance;
        }

        return distance;
    }

    IEnumerator CullIslandsLocally()
    {
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            yield return null;
        }

        while (true)
        {
            yield return null;

            foreach (var spawnedIsland in spawnedIslands)
            {
                var distance = Vector3.Distance(Game.LocalPlayer.transform.position, spawnedIsland.transform.position);
                spawnedIsland.DistanceCull(distance);
                yield return null;
            }
        }
    }

    [Server]
    public void NewIslandSpawned(Island newIsland)
    {
        Debug.Log("NEW ISLAND SPAWNED ON SERVER. " + newIsland);
        int currentSeed = Random.Range(1,999) * Random.Range(1,999) * Random.Range(1,999);
        List<VoxelBuildingFloor.VoxelFloorRandomSettings> voxelFloorsRandomSettings = newIsland.VoxelBuildingGen.RandomizeSettingsOnHost();
        ServerManager.Spawn(newIsland.gameObject);
        RpcInitIslandOnClients(newIsland, currentSeed, voxelFloorsRandomSettings);
    }

    [ObserversRpc(IncludeOwner = true)]
    void RpcInitIslandOnClients(Island newIsland, int currentSeed, List<VoxelBuildingFloor.VoxelFloorRandomSettings> voxelFloorsRandomSettings)
    {
        if (spawnedIslands.Contains(newIsland))
            return;
        
        spawnedIslands.Add(newIsland);
        newIsland.Init(currentSeed, voxelFloorsRandomSettings);
        
    }
    
}
