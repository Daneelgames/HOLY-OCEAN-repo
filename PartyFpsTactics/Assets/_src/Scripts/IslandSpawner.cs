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
    [SerializeField] private List<Island> islandPrefabList = new List<Island>();

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        Instance = this;

        //SpawnIslandOnServer();

        if (base.IsHost)
            StartCoroutine(CullIslandsOnServer());
    }

    private void Update()
    {
        //test
        if (Input.GetKeyDown(KeyCode.L))
            SpawnIslandOnServer();
    }

    [Server]
    void SpawnIslandOnServer()
    {
        var randomIslandPrefab = islandPrefabList[Random.Range(0, islandPrefabList.Count)];
        var newIsland = Instantiate(randomIslandPrefab, new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100)), Quaternion.identity);
 
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
        foreach (var island in spawnedIslands)
        {
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
        var voxelFloorsRandomSettings = newIsland.VoxelBuildingGen.RandomizeSettingsOnHost();
        ServerManager.Spawn(newIsland.gameObject);
        RpcInitIslandOnClients(newIsland, currentSeed, voxelFloorsRandomSettings);
    }

    [ObserversRpc(IncludeOwner = true)]
    void RpcInitIslandOnClients(Island newIsland, int currentSeed, List<VoxelBuildingGenerator.VoxelFloorSettingsRaw> voxelFloorsRandomSettings)
    {
        if (spawnedIslands.Contains(newIsland))
            return;
        
        spawnedIslands.Add(newIsland);
        newIsland.Init(currentSeed, voxelFloorsRandomSettings);
    }
}