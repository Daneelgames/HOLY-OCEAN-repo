using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Fraktalia.VoxelGen.Modify;
using Sirenix.OdinInspector;
using UnityEngine;

public class Island : NetworkBehaviour
{
    [SerializeField] private BuildingGenerator _tileBuildingGenerator;
    [SerializeField] private VoxelBuildingGenerator _voxelBuildingGenerator;
    public VoxelBuildingGenerator VoxelBuildingGen => _voxelBuildingGenerator;
    [SerializeField] private NavMeshSurfaceUpdate _navMeshSurfaceUpdate;

    [BoxGroup("ISLAND LODs")] [SerializeField] [ReadOnly] private float distanceToLocalPlayer;
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner == false)
        {
            IslandSpawner.Instance.NewIslandSpawned(this);
        }   
    }
    public void Init(int seed, List<VoxelBuildingFloor.VoxelFloorRandomSettings> voxelFloorRandomSettings)
    {
        StartCoroutine(InitCoroutine(seed, voxelFloorRandomSettings));
    }

    IEnumerator InitCoroutine(int seed, List<VoxelBuildingFloor.VoxelFloorRandomSettings> voxelFloorRandomSettings)
    {

        _voxelBuildingGenerator?.SaveRandomSeedOnEachClient(seed, voxelFloorRandomSettings);
        yield return null;
        _navMeshSurfaceUpdate.Init();
    }

    public void DistanceCull(float distance)
    {
        // Island LOD system
        // close: update navmeshes, props, mobs 100%
        // mid: activate mobs, island might shoot at you
        // fat: hide everything, show lowpoly LOD
        distanceToLocalPlayer = distance;
    }
}
