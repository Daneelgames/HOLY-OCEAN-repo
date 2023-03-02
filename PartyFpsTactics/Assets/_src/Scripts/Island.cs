using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Fraktalia.VoxelGen;
using Fraktalia.VoxelGen.Modify;
using Fraktalia.VoxelGen.Modify.Procedural;
using MrPink.Health;
using MrPink.Units;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Object = System.Object;
using Random = UnityEngine.Random;

public class Island : NetworkBehaviour
{
    [SerializeField] private List<VoxelSaveSystem> _voxelSaveSystems = new List<VoxelSaveSystem>();
    [SerializeField] private BuildingGenerator _tileBuildingGenerator;
    public BuildingGenerator TileBuildingGenerator => _tileBuildingGenerator;
    [SerializeField] private VoxelBuildingGenerator _voxelBuildingGenerator;
    public VoxelBuildingGenerator VoxelBuildingGen => _voxelBuildingGenerator;
    [SerializeField] private NavMeshSurfaceUpdate _navMeshSurfaceUpdate;
    [SerializeField] private float randomSpherePosOnNavMeshMaxRange = 100;

    [SerializeField] private bool spawnBoss = true;
    private bool bossKilled = false;

    [SerializeField] [ReadOnly] private List<HealthController> islandUnits = new List<HealthController>();

    [BoxGroup("ISLAND LODs")] [SerializeField] [ReadOnly]
    private bool culled = true;

    [BoxGroup("ISLAND LODs")] [SerializeField] [ReadOnly]
    private float distanceToLocalPlayer;

    [BoxGroup("ISLAND LODs")] [SerializeField]
    private float mobsIslandSpawnDistance = 300;

    [BoxGroup("ISLAND LODs")] [SerializeField]
    private float mobsIslandDespawnDistance = 500;

    [SerializeField] private ColliderToVoxel[] voxelCutterForBuildings;

    [BoxGroup("Havok")] [SerializeField] [ReadOnly]
    private int targetHavok;

    [BoxGroup("Havok")] [SerializeField] [ReadOnly]
    private int currentHavok;

    float GetHavokFill => (float)currentHavok / targetHavok;
    public bool IsCulled => culled;
    private float sinkSpeed = 1;

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

    public void DestroyOnRunEnded()
    {
        culled = true;
        _navMeshSurfaceUpdate?.Stop();
        DespawnIslandEnemies();

        if (base.IsHost)
            ServerManager.Despawn(gameObject, DespawnType.Destroy);
    }

    public void Init(int seed, List<VoxelBuildingGenerator.VoxelFloorSettingsRaw> voxelFloorRandomSettings)
    {
        StartCoroutine(InitCoroutine(seed, voxelFloorRandomSettings));
    }

    [Button]
    public void GetVoxelSaveSystems()
    {
        var saveSystems = gameObject.GetComponentsInChildren<VoxelSaveSystem>();
        foreach (var voxelSaveSystem in saveSystems)
        {
            _voxelSaveSystems.Add(voxelSaveSystem);
        }
    }
    
    IEnumerator InitCoroutine(int seed, List<VoxelBuildingGenerator.VoxelFloorSettingsRaw> voxelFloorRandomSettings)
    {

        foreach (var voxelSaveSystem in _voxelSaveSystems)
        {
            voxelSaveSystem.Load();
        }

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
        if (_tileBuildingGenerator && _tileBuildingGenerator.Generated == false)
            return;
        
        if (culled/* && distanceToLocalPlayer <= mobsIslandSpawnDistance*/)
        {
            culled = false;
            _navMeshSurfaceUpdate?.Init();
            SpawnIslandEnemies();
            if (spawnBoss && bossKilled == false)
                MusicManager.Instance.PlayIslandMusic();
            return;
        }
        return; // dont despawn mobs if you're far away - no reason for it
        if (distanceToLocalPlayer >= mobsIslandDespawnDistance)
        {
            if (culled == false)
            {
                culled = true;
                _navMeshSurfaceUpdate?.Stop();
                DespawnIslandEnemies();
            }
            
            if (base.IsHost && spawnBoss && bossKilled)
            {
                spawnBoss = false;
                ServerManager.Despawn(gameObject, DespawnType.Destroy);
                return;
            }
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

        if (spawnBoss && bossKilled == false)
        {
            if (islandHavokCoroutine != null)
                StopCoroutine(islandHavokCoroutine);
            InitTargetHavok();
            islandHavokCoroutine = StartCoroutine(GetIslandHavok());
        }
    }

    void InitTargetHavok()
    {
        targetHavok = 0;
        currentHavok = 0;
        foreach (var level in _tileBuildingGenerator.spawnedBuildings[0].spawnedBuildingLevels)
        {
            for (var index = 0; index < level.unitsToSpawn.Count; index++)
            {
                targetHavok++;
            }
        }

        targetHavok = Mathf.RoundToInt(targetHavok * 0.8f); // kill most mobs to spawn boss
    }

    
    private Coroutine islandHavokCoroutine;
    IEnumerator GetIslandHavok()
    {
        IslandHavokUi.Instance.ShowBar();
        while (currentHavok < targetHavok)
        {
            float t = 1;
            while (t > 0)
            {
                t -= Time.fixedUnscaledDeltaTime;
                IslandHavokUi.Instance.SetHavokFill(GetHavokFill);
                yield return null;
            }
            if (distanceToLocalPlayer > mobsIslandDespawnDistance)
            {
                // reduce havok if player is far
                currentHavok = Mathf.Clamp(currentHavok - 1, 0, targetHavok);
            }
        }
        
        IslandHavokUi.Instance.HideBar();
        IslandHavokFull();
        StopCoroutine(islandHavokCoroutine);
    }


    public void IslandHavokFull()
    {
        ContentPlacer.Instance.SpawnBossOnIsland(this, _tileBuildingGenerator.GetRandomPosInsideLastLevel());
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

    public void AddIslandUnit(HealthController unit, bool boss = false)
    {
        if (islandUnits.Contains(unit)) return;   
        
        islandUnits.Add(unit);
        if (boss)
            unit.OnDeathEvent.AddListener(HealthController_OnBossKilled);
        else
            unit.OnDeathEvent.AddListener(HealthController_OnIslandUnitKilled);
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

        StartCoroutine(CutVoxelsForRoomsOverTime());
    }

    IEnumerator CutVoxelsForRoomsOverTime()
    {
        Debug.Log("CutVoxelsForRoomsOverTime; voxelCutterForBuildings " + voxelCutterForBuildings.Length);
        
        var manualBaker = voxelCutterForBuildings[0].gameObject.GetComponent<ColliderToVoxelManualBaker>();
        
        foreach (var colliderToVoxel in voxelCutterForBuildings)
        {
            yield return null;

            while (colliderToVoxel.TargetGenerator.HullGeneratorsWorking)
            {
                yield return null;
            }
            
            if (manualBaker)
            {
                manualBaker._colliderToVoxel = colliderToVoxel;
                manualBaker.HideAllColliders();
                manualBaker.Bake(colliderToVoxel.transform.childCount, true);
                continue;
            }
            
            colliderToVoxel.ApplyProceduralModifier(true);
        }
    }

    void HealthController_OnBossKilled()
    {
        
        UiMarkWorldObject islandMarker = gameObject.GetComponent<UiMarkWorldObject>();
        Destroy(islandMarker);

        bossKilled = true;
        MusicManager.Instance.PlayIslandMusic();
        
        
        ProgressionManager.Instance.LevelCompleted();
        /*
        if (base.IsHost)
            ServerManager.Despawn(gameObject, DespawnType.Destroy);*/
        //Destroy(gameObject);
        StartCoroutine(SinkIsland());
    }

    IEnumerator SinkIsland()
    {
        /*
        while (transform.position.y > - 500)
        {
            yield return null;
            transform.position += Vector3.down * sinkSpeed;
        }
        */
        yield return null;
        if (base.IsHost)
            ServerManager.Despawn(gameObject, DespawnType.Destroy);
    }
    void HealthController_OnIslandUnitKilled()
    {
        currentHavok++;
    }
}
