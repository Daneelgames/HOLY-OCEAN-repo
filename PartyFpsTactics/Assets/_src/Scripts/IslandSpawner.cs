using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
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
    [SerializeField] private float spawnDistance = 1000;
    [SerializeField] private List<Island> islandPrefabList = new List<Island>();

    List<BuildingGenerator> TileBuildingsInstances = new List<BuildingGenerator>();

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        Instance = this;

        //SpawnIslandOnServer();

        if (base.IsHost)
            StartCoroutine(CullIslandsOnServer());
    }

    public void AddTileBuilding(BuildingGenerator buildingGenerator)
    {
        if (TileBuildingsInstances.Contains(buildingGenerator))
            return;
        TileBuildingsInstances.Add(buildingGenerator);
    }
    public void RemoveTileBuilding(BuildingGenerator buildingGenerator)
    {
        if (TileBuildingsInstances.Contains(buildingGenerator) == false)
            return;
        TileBuildingsInstances.Remove(buildingGenerator);
    }
    
    public BuildingGenerator GetClosestTileBuilding(Vector3 pos)
    {
        BuildingGenerator closest = null;
        float distance = 10000;
        foreach (var instance in TileBuildingsInstances)
        {
            if (instance == null)
                continue;
            
            var newDistance = Vector3.Distance(instance.transform.position, pos);
            if (newDistance < distance)
            {
                distance = newDistance;
                closest = instance;
            }
        }

        return closest;
    }

    [Server]
    public void SpawnRandomIslandOnServer()
    {
        int islandIndex = ProgressionManager.Instance.currentLevelIndex;
        islandIndex = Mathf.Clamp(islandIndex, 0, islandPrefabList.Count - 1);
        var randomIslandPrefab = islandPrefabList[islandIndex];
        var spawnDir = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100)).normalized;
        
        var spawnPos = spawnDir * spawnDistance;
        var newIsland = Instantiate(randomIslandPrefab, spawnPos, Quaternion.identity);
 
        ServerManager.Spawn(newIsland.gameObject);
    }

    /*
    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);
        /* Current owner can be found by using base.Owner. prevOwner
        * contains the connection which lost ownership. Value will be
        * -1 if there was no previous owner. #1#

        StartCoroutine(CullIslandsOnServer());
    }*/

    public float GetDistanceToClosestIsland(Vector3 posAsking)
    {
        float distance = 100000f;
        if (spawnedIslands.Count < 1)
            return distance;
        for (var index = spawnedIslands.Count - 1; index >= 0; index--)
        {
            var island = spawnedIslands[index];
            if (island == null)
            {
                spawnedIslands.RemoveAt(index);
                continue;
            }
            var pos = island.gameObject.transform.position;
            var newDistance = Vector3.Distance(pos, posAsking);
            if (newDistance < distance)
                distance = newDistance;
        }

        return distance;
    }

    [Server]
    IEnumerator CullIslandsOnServer()
    {
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            yield return null;
        }

        float closestDistance = 100000;
        while (true)
        {
            yield return null;
            
            if (spawnedIslands.Count < 1)
                continue;
            
            for (var index = spawnedIslands.Count - 1; index >= 0; index--)
            {
                var spawnedIsland = spawnedIslands[index];
                if (spawnedIsland == null)
                    continue;
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
        ServerManager.Spawn(newIsland.gameObject);
        
        int currentSeed = Random.Range(1,999) * Random.Range(1,999) * Random.Range(1,999);
        if (newIsland.VoxelBuildingGen)
        {
            var voxelFloorsRandomSettings = newIsland.VoxelBuildingGen.RandomizeSettingsOnHost();
            RpcInitIslandOnClients(newIsland, currentSeed, voxelFloorsRandomSettings);
        }
        else
        {
            RpcInitIslandOnClients(newIsland, currentSeed, null);
        }
    }

    public void IslandDestroyed(Island island)
    {
        if (spawnedIslands.Contains(island) == false)
            return;
        spawnedIslands.Remove(island);
    }

    [ObserversRpc(IncludeOwner = true)]
    void RpcInitIslandOnClients(Island newIsland, int currentSeed, List<VoxelBuildingGenerator.VoxelFloorSettingsRaw> voxelFloorsRandomSettings)
    {
        if (spawnedIslands.Contains(newIsland))
            return;
        
        if (spawnedIslands.Count > 1)
        {
            for (int i = spawnedIslands.Count - 1; i >= 0; i--)
            {
                var island = spawnedIslands[i];
                if (island == null)
                    spawnedIslands.RemoveAt(i);
            }
        }
        
        spawnedIslands.Add(newIsland);
        newIsland.Init(currentSeed, voxelFloorsRandomSettings);
    }
}