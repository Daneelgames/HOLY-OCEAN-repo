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
    [SyncVar][SerializeField] [ReadOnly] int currentSeed_s;
    [SerializeField] [ReadOnly] private List<Island> spawnedIslands = new List<Island>();
    public int CurrentSeed => currentSeed_s;
    [SyncVar][SerializeField][ReadOnly] List<VoxelBuildingFloor.VoxelFloorRandomSettings> voxelFloorsRandomSettings_s;

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        Instance = this;
        StartCoroutine(CullIslandsLocally());
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
        ServerManager.Spawn(newIsland.gameObject);
        RpcIniIslandOnClients(newIsland);
    }

    [ObserversRpc(IncludeOwner = true)]
    void RpcIniIslandOnClients(Island newIsland)
    {
        if (spawnedIslands.Contains(newIsland))
            return;
        
        spawnedIslands.Add(newIsland);
        newIsland.Init(currentSeed_s, voxelFloorsRandomSettings_s);
        
    }
    
}
