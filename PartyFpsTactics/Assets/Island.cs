using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Fraktalia.VoxelGen.Modify;
using UnityEngine;

public class Island : NetworkBehaviour
{
    [SerializeField] private List<VoxelModifier> islandVoxelModifiers = new List<VoxelModifier>();
    [SerializeField] private BuildingGenerator _tileBuildingGenerator;
    [SerializeField] private VoxelBuildingGenerator _voxelBuildingGenerator;
    [SerializeField] private NavMeshSurfaceUpdate _navMeshSurfaceUpdate;
    
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
        foreach (var islandVoxelModifier in islandVoxelModifiers)
        {
            yield return null;
            GameVoxelModifier.Instance.AddIslandModifier(islandVoxelModifier);
        }

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
        return;
        foreach (var islandVoxelModifier in islandVoxelModifiers)
        {
            GameVoxelModifier.Instance.RemoveIslandModifier(islandVoxelModifier);
        }
    }
}
