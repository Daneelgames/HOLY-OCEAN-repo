using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Fraktalia.VoxelGen.Modify;
using Fraktalia.VoxelGen.Modify.Procedural;
using MrPink.Health;
using MrPink.Units;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Island : NetworkBehaviour
{
    [SerializeField] private BuildingGenerator _tileBuildingGenerator;
    [SerializeField] private VoxelBuildingGenerator _voxelBuildingGenerator;
    public VoxelBuildingGenerator VoxelBuildingGen => _voxelBuildingGenerator;
    [SerializeField] private NavMeshSurfaceUpdate _navMeshSurfaceUpdate;
    [SerializeField] private float randomSpherePosOnNavMeshMaxRange = 100;
    
    [SerializeField] private bool spawnBoss = true;

    [SerializeField] [ReadOnly] private List<HealthController> islandUnits = new List<HealthController>();
    [BoxGroup("ISLAND LODs")] [SerializeField] [ReadOnly] private bool culled = true;
    [BoxGroup("ISLAND LODs")] [SerializeField] [ReadOnly] private float distanceToLocalPlayer;
    [BoxGroup("ISLAND LODs")] [SerializeField] private float mobsIslandSpawnDistance = 300;
    [BoxGroup("ISLAND LODs")] [SerializeField] private float mobsIslandDespawnDistance = 500;

    [SerializeField] private ColliderToVoxel[] voxelCutterForBuildings;
    public bool IsCulled => culled;
    
    public override void OnStartClient()
    {
        base.OnStartClient();
        StartCoroutine(AddIslandToSpawner());
    }

    IEnumerator AddIslandToSpawner()
    {
        while (IslandSpawner.Instance == null) yield return null;
        
        IslandSpawner.Instance.NewIslandSpawned(this);
    }

    private void OnDestroy()
    {
        IslandSpawner.Instance.IslandDestroyed(this);
    }

    public void Init(int seed, List<VoxelBuildingGenerator.VoxelFloorSettingsRaw> voxelFloorRandomSettings)
    {
        StartCoroutine(InitCoroutine(seed, voxelFloorRandomSettings));
    }

    IEnumerator InitCoroutine(int seed, List<VoxelBuildingGenerator.VoxelFloorSettingsRaw> voxelFloorRandomSettings)
    {
        _tileBuildingGenerator?.InitOnClient(seed, this);
        _voxelBuildingGenerator?.SaveRandomSeedOnEachClient(seed, voxelFloorRandomSettings);
        yield return null;
    }

    // this one calls often locally on server
    [Server]
    public void DistanceCull(float distance)
    {
        // Island LOD system
        // close: update navmeshes, props, mobs 100%
        // mid: activate mobs, island might shoot at you
        // fat: hide everything, show lowpoly LOD
        distanceToLocalPlayer = distance;

        if (culled && distanceToLocalPlayer <= mobsIslandSpawnDistance)
        {
            culled = false;
            _navMeshSurfaceUpdate?.Init();
            SpawnIslandEnemies();
            return;
        }

        if (culled == false && distanceToLocalPlayer >= mobsIslandDespawnDistance)
        {
            culled = true;
            _navMeshSurfaceUpdate?.Stop();
            DespawnIslandEnemies();
            return;
        }
    }

    [Server]
    void SpawnIslandEnemies()
    {
        if (_voxelBuildingGenerator)
        {
            StartCoroutine(ContentPlacer.Instance.SpawnEnemiesInVoxelBuilding(_voxelBuildingGenerator.Floors, this));
        }
        if (_tileBuildingGenerator)
        {
            ContentPlacer.Instance.SpawnEnemiesInBuilding(_tileBuildingGenerator.spawnedBuildings[0], this);
        }
        if (spawnBoss)
            ContentPlacer.Instance.SpawnBossOnIsland(this, GetRandomPosOnNavMesh());
    }

    Vector3 GetRandomPosOnNavMesh()
    {
        Vector3 pos = transform.position + Vector3.up * (randomSpherePosOnNavMeshMaxRange / 2) + Random.insideUnitSphere * Random.Range(0,randomSpherePosOnNavMeshMaxRange);
        if (NavMesh.SamplePosition(pos, out var hit, Mathf.Infinity, NavMesh.AllAreas))
        {
            pos = hit.position;
        }
        return pos;
    }
    
    [Server]
    void DespawnIslandEnemies()
    {
        for (var index = 0; index < islandUnits.Count; index++)
        {
            var unit = islandUnits[index];
            if (unit == null) continue;
            
            ServerManager.Despawn(unit.gameObject, DespawnType.Destroy);
        }
        islandUnits.Clear();
    }

    public void AddIslandUnit(HealthController unit)
    {
        if (islandUnits.Contains(unit)) return;   
        
        islandUnits.Add(unit);
    }
    void SpawnPropsInBuilding()
    {
        if (_voxelBuildingGenerator)
        {
            StartCoroutine(ContentPlacer.Instance.SpawnPropsInVoxelBuilding(_voxelBuildingGenerator.Floors));
        }
    }

    public void AddRoomCutter(GameObject cutDummy)
    {
        if (voxelCutterForBuildings.Length > 0)
            cutDummy.transform.parent = voxelCutterForBuildings[0].transform;
    }

    public void BuildingGenerated()
    {
        // cut rooms
        //if (voxelCutterForBuildings[0].colliders.Length <= 0) return;
        
        foreach (var colliderToVoxel in voxelCutterForBuildings)
        {
            colliderToVoxel.ApplyProceduralModifier(true);
        }
    }
}
